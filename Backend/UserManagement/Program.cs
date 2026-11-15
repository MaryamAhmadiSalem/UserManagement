using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Application.Extensions;
using UserManagement.Application.Interfaces;
using UserManagement.Configuration;
using UserManagement.Configuration.Authorization;
using UserManagement.Configuration.Extensions;
using UserManagement.Endpoints;
using UserManagement.Infrastructure;
using UserManagement.Infrastructure.Repository;
using UserManagement.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//add configurations with IConfiguration
builder.Services.AddConfiguration(builder.Configuration);

//add configuration information with Configure(pattern option)
builder.Services.AddDbContext<AppDbContext>((ServiceProvider, options) =>
{
    var databaseSetting = ServiceProvider.GetRequiredService<IOptions<DatabaseSetting>>().Value;
    options.UseSqlServer(databaseSetting.DefaultConnection);
});

builder.Services.AddApplication();

builder.Services.AddIdentityCore<IdentityUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITokenBlackListService, TokenBlackListService>();
builder.Services.AddScoped<IdentitySeeder>();

builder.Services.AddValidation();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddAuthentication()
    .AddJwtBearer(option =>
        {
            //option.MapInboundClaims = false;
            option.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = builder.Configuration["Jwt:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                      Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
                ),
                ValidateLifetime = true,
                RoleClaimType = ClaimTypes.Role
            };
            option.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var tokenBlackListService = context.HttpContext.RequestServices.GetRequiredService<ITokenBlackListService>();
                    //var claims = context.Principal?.Claims;
                    var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                    //var jti = context.HttpContext.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                    var isTokenExist = await tokenBlackListService.IsTokenBlackList(jti!);
                    if (isTokenExist)
                        context.Fail("Token has been revoked");
                }
            };
        }
    );
builder.Services.AddAuthorization(option =>
{
    option.AddPolicy("UserRead", policy =>
        policy.RequireClaim("permission", UserPermissions.ReadUsers)
    );
    option.AddPolicy("UserCreate", policy =>
        policy.RequireClaim("permission", UserPermissions.CreateUsers)
    );
    option.AddPolicy("UserUpdate", policy =>
        policy.RequireClaim("permission", UserPermissions.UpdateUsers)
    );
    option.AddPolicy("UserDelete", policy =>
        policy.RequireClaim("permission", UserPermissions.DeleteUsers)
    );
    option.AddPolicy("RolesAssign", policy =>
        policy.RequireClaim("permission", UserPermissions.AssignRoles)
    );
    option.AddPolicy("ReadAllUsers", policy =>
        policy.RequireRole("Manager", "Admin")
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AngularFrontend");

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
    await seeder.CreateRoles();
    await seeder.CreateManager();
    await seeder.CreateRoleClaim();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapUserEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();