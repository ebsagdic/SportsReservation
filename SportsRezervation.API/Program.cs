using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SportsReservation.Core.Abstract;
using SportsReservation.Core.Abstract.Services;
using SportsReservation.Core.Models;
using SportsReservation.Repository.Context;
using SportsReservation.Repository.Repositories;
using SportsReservation.Repository.UnitOfWork;
using SportsReservation.Service;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Swagger i�in Bearer token giri�i ekleme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        //Type = SecuritySchemeType.ApiKey// A�a��daki 3 de�er, swagger �zerinden istek at�l�rken Bearer header�na ihtiya� kalmamas� i�in eklendi.
        Type = SecuritySchemeType.Http,  // Burada de�i�iklik yap�ld�
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddHangfire(x => x.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthenticationService, AuthanticationService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.Configure<CustomTokenOption>(builder.Configuration.GetSection("TokenOption"));
builder.Services.AddIdentity<CustomUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{ 
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    //JWT tabanl� kimlik do�rulaman�n varsay�lan kimlik do�rulama ve
    //challenge (kimlik do�rulama iste�i) �emas�n� tan�mlar.
}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
// AddJwtBearer,�stemciden gelen token�lar� otomatik olarak do�rular ve bu token�lar�n ge�erli olup olmad���n� kontrol eder.
//AddJwtBearer,Token��n ge�erlilik s�resi, imzas�, issuer ve audience gibi bilgilerini kontrol ederek, istemcinin g�venilirli�ini sa�lar.
{
    var tokenOptions = builder.Configuration.GetSection("TokenOption").Get<CustomTokenOption>();

    if (tokenOptions == null)
    {
        throw new ArgumentNullException("TokenOption configuration is missing in appsettings.json.");
    }

    if (string.IsNullOrEmpty(tokenOptions.SecurityKey))
    {
        throw new ArgumentNullException("SecurityKey", "The TokenOption:SecurityKey value cannot be null or empty.");
    }


    opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
    {
        ValidIssuer = tokenOptions.Issuer,
        ValidAudience = tokenOptions.Audience.FirstOrDefault(),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.SecurityKey)),

        ValidateIssuerSigningKey = true,
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role,
    };
});

var app = builder.Build();

app.UseHangfireDashboard();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//RecurringJob.AddOrUpdate<IReservationService>("CancelUnpaidReservationsAsync",
//    service => service.CancelUnpaidReservationsAsync(), "0 */10 * * * *");

//E�er RecurringJob.AddOrUpdate metodunu yukar�daki gibi Program.cs  i�inde do�rudan �a��r�yorsan, Hangfire��n hala initialize edilmemi� olma ihtimali var. Bunu service provider �zerinden �a��rmal�s�n
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate(
        "CancelUnpaidReservationsAsync",
        () => scope.ServiceProvider.GetRequiredService<IReservationService>().CancelUnpaidReservationsAsync(),
        "0 */10 * * * *");
}
app.Run();
