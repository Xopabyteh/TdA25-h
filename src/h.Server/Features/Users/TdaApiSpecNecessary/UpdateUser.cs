using Carter;
using h.Contracts;
using h.Contracts.Users;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Auth;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Features.Users.TdaApiSpecNecessary;

public static class UpdateUser
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/v1/users/{id}", Handle);
        }
    }

    public static async Task<IResult> Handle(
        [FromServices] AppDbContext db,
        [FromServices] PasswordHashService passwordHashService,
        [FromServices] UserService userService,
        [FromRoute] Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await db.UsersDbSet.FirstOrDefaultAsync(u => u.Uuid == id, cancellationToken);
        if (user is null)
            return ErrorResults.NotFound([SharedErrors.User.UserNotFound()]);

        // Check if user exists
        var nicknameTakenErrors = await userService.NicknameAlreadyRegistered(
            request.Username,
            request.Email,
            cancellationToken,
            excludedId: user.Uuid);

        if (nicknameTakenErrors is not null)
            return ErrorResults.Conflit("The user already exists", nicknameTakenErrors);
        
        var newPasswordHash = passwordHashService.GetPasswordHash(request.Password);

        user.Username = request.Username;
        user.Email = request.Email;
        user.Elo = new()
        {
            Rating = request.Elo
        };
        user.PasswordHash = newPasswordHash;

        db.Update(user);    
        await db.SaveChangesAsync(cancellationToken);

        return Results.Ok();
    }
}
