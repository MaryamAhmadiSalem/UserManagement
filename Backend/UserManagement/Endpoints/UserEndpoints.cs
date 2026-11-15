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

            app.MapGet("/api/users", async (IUserService service, HttpContext httpContext) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userInfo = await service.ReadAllUser(userId!);
                return Results.Ok(userInfo.Data);
            }).RequireAuthorization("UserRead", "ReadAllUsers");

            app.MapGet("/api/users/{id}", async (IUserService service, int id, HttpContext httpContext) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await service.ReadOneUser(id, userId);
                if (user.IsSuccess) return Results.Ok(user.Data);
                if (user.Error!.Contains("Forbidden")) return Results.Forbid();
                return Results.NotFound();
            }).RequireAuthorization("UserRead");

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
            }).RequireAuthorization("UserUpdate");

            app.MapDelete("/api/users/{id}", async (IUserService service, int id, HttpContext httpContext) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await service.DeleteUser(id, userId!);
                if (user.IsSuccess) return Results.NoContent();
                if (user.Error!.Contains("Forbidden"))
                    return Results.Forbid();
                return Results.NotFound("User not found!");
            }).RequireAuthorization("UserDelete");

            app.MapGet("/api/users/search/{nationalId}", async (IUserService service, string nationalId, HttpContext httpContext) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await service.GetByNationalId(nationalId, userId);
                if (user.IsSuccess) return Results.Ok(user.Data);
                if (user.Error!.Contains("Forbidden")) return Results.Forbid();
                return Results.NotFound();
            }).RequireAuthorization("UserRead");

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

            app.MapPost("/api/auth/logout", async (HttpContext httpContext, ITokenBlackListService tokenService, RefreshTokenDto dto, IUserService service) =>
            {
                var jti = httpContext.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                var expString = httpContext.User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
                var exp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expString!));
                var isTokenAdd = await tokenService.AddTokenToDatabase(jti!, exp);
                if (isTokenAdd)
                {
                    await service.TokenRevocation(dto);
                    return Results.Ok();
                }
                return Results.NotFound();
            }).RequireAuthorization();

            app.MapPost("/api/auth/refresh-expired", async (RefreshTokenDto dto, IUserService service) =>
            {
                var tokenRevoked = await service.TokenRevocation(dto);
                if (tokenRevoked) return Results.Ok();
                return Results.Unauthorized();
            });

            app.MapPost("/api/users/assign-role", async (AssignRoleDto dto, IUserService service) =>
            {
                var assignRoleUser = await service.AssignRole(dto);
                if (assignRoleUser.IsSuccess) return Results.Ok();
                return Results.BadRequest(assignRoleUser);
            }).RequireAuthorization("RolesAssign");

            app.MapPost("/api/token/renewal", async (RefreshTokenDto dto, IUserService service) =>
            {
                var renewalToken = await service.TokenRenewal(dto);
                if (renewalToken.IsSuccess) return Results.Ok(renewalToken.Data);
                if (renewalToken.Error!.Contains("NotFound")) return Results.NotFound();
                else return Results.Unauthorized();
            });
        }
    }
}