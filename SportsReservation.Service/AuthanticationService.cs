using Microsoft.AspNetCore.Identity;
using SportsReservation.Core.Abstract;
using SportsReservation.Core.Models;
using SportsReservation.Core.Abstract.Services;
using SportsReservation.Core.Models.DTO_S;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace SportsReservation.Service
{
    public class AuthanticationService : IAuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<CustomUser> _userManager;
        private readonly CustomTokenOption _customTokenOption;
        private readonly IGenericRepository<UserRefreshToken> _genericRepository;
        public AuthanticationService(IUnitOfWork unitOfWork, UserManager<CustomUser> userManager,IOptions<CustomTokenOption> customTokenOption, IGenericRepository<UserRefreshToken> genericRepository)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _customTokenOption = customTokenOption.Value;
            _genericRepository = genericRepository;
        }

        public async Task<Response<TokenDto>> LoginAsync(UserForLoginDto loginDto)
        {
            var errors = new List<string>();
            if(loginDto == null)
            {
                errors.Add("Kullanıcı bilgisi bulunamadı");
                return Response<TokenDto>.Fail(404, errors);
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email.ToUpper());

            if (user == null) 
            {
                errors.Add("Bu mail adresine ait kullanıcı bulunamadı");
                return Response<TokenDto>.Fail(400, errors);
            }

            var userRoles = (await _userManager.GetRolesAsync(user)).ToList();
            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                errors.Add("Email veya şifre eşleşmiyor");
                return Response<TokenDto>.Fail(400,errors);
            }
            TokenDto tokenDto = CreateToken(user,userRoles);

            var userRefreshToken =  _genericRepository.FindByCondition(x => x.UserId == user.Id).SingleOrDefault();

            if (userRefreshToken == null)
            {
                await _genericRepository.CreateAsync(new UserRefreshToken { CreateUser = user.Name, UserId = user.Id, Code = tokenDto.RefreshToken, Expiration = tokenDto.RefreshTokenExpiration });
            }
            else
            {
                userRefreshToken.Code = tokenDto.RefreshToken;
                userRefreshToken.Expiration = tokenDto.RefreshTokenExpiration;
                userRefreshToken.CreateUser = user.Name;
                _genericRepository.Update(userRefreshToken);
            }
            await _unitOfWork.CommitAsync();
            return Response<TokenDto>.Success(tokenDto, 200);
        }

        public async Task<Response<TokenDto>> CreateTokenByRefreshToken(string refreshToken)
        {
            var existRefreshToken = _genericRepository.FindByCondition(x => x.Code == refreshToken).SingleOrDefault();
            if (existRefreshToken == null)
            {
                return Response<TokenDto>.Fail(400, new List<string> { "Bu refresh token mevcut değil" });
            }
            var user = await _userManager.FindByIdAsync(existRefreshToken.UserId);
            if (user == null) 
            {
                return Response<TokenDto>.Fail(400, new List<string> { "Bu kullanıcı mevcut değil" });
            }
            var userRoles = (await _userManager.GetRolesAsync(user)).ToList();
            var tokenDto = CreateToken(user, userRoles);

            existRefreshToken .Code = tokenDto.RefreshToken;
            existRefreshToken .Expiration = tokenDto.RefreshTokenExpiration;
            _genericRepository.Update(existRefreshToken);

            await _unitOfWork.CommitAsync();

            return Response<TokenDto>.Success(tokenDto, 200);

        }

        public async Task<Response<NoDataDto>> RevokeRefreshToken(string refreshToken)
        {
            var existRefreshToken =  _genericRepository.FindByCondition(x => x.Code == refreshToken).SingleOrDefault();
            if (existRefreshToken == null)
            {
                return Response<NoDataDto>.Fail(404, new List<string> { "Refresh Token Bulunamadı" });
            }
            _genericRepository.Delete(existRefreshToken);
            _unitOfWork.CommitAsync();
            return Response<NoDataDto>.Success(null, 200);
        }


        public TokenDto CreateToken(CustomUser user, List<string> userRoles)
        {
            var accessTokenExpiration = DateTime.UtcNow.AddMinutes(_customTokenOption.AccessTokenExpiration);
            var refreshTokenExpiration = DateTime.UtcNow.AddMinutes(_customTokenOption.RefreshTokenExpiration);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_customTokenOption.SecurityKey));
            SigningCredentials signingCredentials = new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha256Signature);

            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                issuer:_customTokenOption.Issuer,
                audience:_customTokenOption.Audience.FirstOrDefault(),
                expires:accessTokenExpiration,
                notBefore:DateTime.Now,
                claims: GetClaims(user,userRoles,_customTokenOption.Audience),
                signingCredentials:signingCredentials
                );
            var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            var tokenDto = new TokenDto { AccessToken = token, RefreshToken = CreateRefreshToken(), AccessTokenExpiration = accessTokenExpiration, RefreshTokenExpiration = refreshTokenExpiration  };

            return tokenDto;
        }
        private IEnumerable<Claim> GetClaims(CustomUser user,List<string> userRoles, List<string> audience)
        {
            var userList = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            userList.AddRange(userRoles.Select(x => new Claim(ClaimTypes.Role, x)));
            userList.AddRange(audience.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x)));
            return userList;
        }
        private string CreateRefreshToken()
        {
            var numberByte = new Byte[32];
            using var rnd = RandomNumberGenerator.Create();
            rnd.GetBytes(numberByte);
            return Convert.ToBase64String(numberByte);
        }
    }
}
