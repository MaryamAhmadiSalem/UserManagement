using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserManagement.Application.DTO;
using UserManagement.Application.Interfaces;

namespace UserManagement.Endpoints
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/users", async (IUserService service, CreateUserDto dto, HttpContext httpContext) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await service.CreateUser(dto, userId!);
                if (user.IsSuccess)
                    return Results.Created($"/api/users/{user.Data?.Id}", user.Data);
                if (user.Error!.Any(e => e is "NationalId" or "Email" or "NationalIdAndEmail"))
                    return Results.Conflict(user.Error);
                return Results.BadRequest(user.Error);
            }).RequireAuthorization();

            app.MapGet("/api/users", async (IUserService service) =>
            {
                var userInfo = await service.ReadAllUser();
                return Results.Ok(userInfo.Data);
            }).RequireAuthorization();

            app.MapGet("/api/users/{id}", async (IUserService service, int id, HttpContext httpContext) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await service.ReadOneUser(id, userId);
                if (user.IsSuccess) return Results.Ok(user.Data);
                if (user.Error!.Contains("Forbidden")) return Results.NotFound();
                return Results.Forbid();
            }).RequireAuthorization();

            app.MapPut("/api/users/{id}", async (IUserService service, CreateUserDto dto, int id, HttpContext httpContext) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await service.UpdateUser(dto, id, userId!);
                if (user.IsSuccess)
                    return Results.NoContent();
                if (user.Error!.Contains("NotFound"))
                    return Results.NotFound("User is not found.");
                if (user.Error!.Any(e => e is "NationalId" or "Email" or "NationalIdAndEmail"))
                    return Results.Conflict(user.Error);
                if (user.Error!.Contains("Forbidden"))
                    return Results.Forbid();
                return Results.BadRequest(user.Error);
            }).RequireAuthorization();

            app.MapDelete("/api/users/{id}", async (IUserService service, int id, HttpContext httpContext) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await service.DeleteUser(id, userId!);
                if (user.IsSuccess) return Results.NoContent();
                if (user.Error!.Contains("Forbidden"))
                    return Results.Forbid();
                return Results.NotFound("User not found!");
            }).RequireAuthorization();

            app.MapGet("/api/users/search/{nationalId}", async (IUserService service, string nationalId, HttpContext httpContext) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await service.GetByNationalId(nationalId, userId);
                if (user.IsSuccess) return Results.Ok(user.Data);
                if (user.Error!.Contains("Forbidden")) return Results.Forbid();
                return Results.NotFound();
            }).RequireAuthorization();

            app.MapPost("/api/register/user", async (IUserService service, RegisterDto dto) =>
            {
                var register = await service.Register(dto);
                if (register.IsSuccess) return Results.Ok(register.Data);
                if (register.Error!.Contains("RegisterFailed")) return Results.Unauthorized();
                return Results.BadRequest(register.Error);
            });
            app.MapPost("/api/login/user", async (IUserService service, LoginDto dto) =>
            {
                var login = await service.Login(dto);
                if (login.IsSuccess) return Results.Ok(login.Data);
                if (login.Error!.Contains("NotFound")) return Results.NotFound();
                if (login.Error!.Contains("IncorrectPassword")) return Results.Unauthorized();
                return Results.BadRequest(login.Error);
            });
            app.MapPost("/api/auth/logout", async (HttpContext httpContext, ITokenBlackListService tokenService) =>
            {
                var jti = httpContext.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                var expString = httpContext.User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
                var exp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expString!));
                var isTokenAdd = await tokenService.AddTokenToDatabase(jti!, exp);
                if(isTokenAdd)
                    return Results.Ok();
                return Results.NotFound();
            }).RequireAuthorization();
        }
    }
}