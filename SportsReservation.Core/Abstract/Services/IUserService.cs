using SportsReservation.Core.Models.DTO_S;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsReservation.Core.Abstract.Services
{
    public interface IUserService
    {
        Task<Response<UserDto>> CreateUserAsync(UserForRegisterDto userForRegisterDto);

        Task<Response<UserDto>> GetUserByNameAsync(string userName);
        Response<List<string>> GetAllRoles();
        Task<Response<UserRoleDto>> AddUserRole(UserRoleDto newUserRole);
    }
}
