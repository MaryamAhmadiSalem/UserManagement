using UserManagement.Application;

namespace UserManagement.Endpoints
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/users", async (IUserService service) =>
            {
                var userInfo = await service.ReadAllUser();
                return Results.Ok(userInfo.Data);
            });

            app.MapGet("/api/users/{id}", async (IUserService service, int id) =>
            {
                var user = await service.ReadOneUser(id);

                if (user.IsSuccess)
                {
                    return Results.Ok(user.Data);
                }
                else
                {
                    return Results.NotFound();
                }
            });

            app.MapPost("/api/users", async (IUserService service, CreateUserDto dto) =>
            {
                var user = await service.CreateUser(dto);
                if (user.IsSuccess)
                {
                    return Results.Created($"/api/users/{user.Data?.Id}", user.Data);
                }
                else
                {
                    return Results.Conflict(user.Error);
                }
            });

            app.MapPut("/api/users/{id}", async (IUserService service, CreateUserDto dto, int id) =>
            {
                var user = await service.UpdateUser(dto, id);
                if (user.IsSuccess)
                {
                    return Results.NoContent();
                }
                else if (user.Error == "NotFound")
                {
                    return Results.NotFound("user is not found.");
                }
                else
                {
                    return Results.Conflict(user.Error);
                }
            });

            app.MapDelete("/api/users/{id}", async (IUserService service, int id) =>
            {
                var user = await service.DeleteUser(id);
                if (user.IsSuccess)
                {
                    return Results.NoContent();
                }
                else
                {
                    return Results.NotFound("User not found!");
                }
            });

            app.MapGet("/api/users/search/{nationalId}", async (string nationalId, IUserService service) =>
            {
                var user = await service.GetByNationalId(nationalId);
                if (user.IsSuccess)
                {
                    return Results.Ok(user.Data);
                }
                else
                {
                    return Results.NotFound();

                }
            });
        }
    }
}