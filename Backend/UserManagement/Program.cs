using UserManagement.Endpoints;
using UserManagement.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UserManagement.Configuration.Extensions;
using UserManagement.Application.Extensions;
using UserManagement.Application.Interfaces;
using UserManagement.Infrastructure;
using UserManagement.Infrastructure.Repository;
using UserManagement.Infrastructure.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

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

builder.Services.AddIdentityCore<IdentityUser>().AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITokenBlackListService, TokenBlackListService>();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AngularFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapUserEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();