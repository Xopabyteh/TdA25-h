using Carter;
using h.Contracts;
using h.Contracts.Users;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Auth;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Features.Users.TdaApiSpecNecessary;

public static class UpdateUserSelf
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/v1/users/self", Handle)
                .RequireAuthorization();
        }
    }

    public static async Task<IResult> Handle(
        [FromBody] UpdateUserSelfRequest request,
        [FromServices] AppDbContext db,
        [FromServices] PasswordHashService passwordHashService,
        [FromServices] UserService userService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var currentUserId = context.User.GetUserId();

        var user = await db.UsersDbSet.FirstOrDefaultAsync(u => u.Uuid == currentUserId, cancellationToken);
        if (user is null)
            return ErrorResults.NotFound([SharedErrors.User.UserNotFound()]);

        // Check if user exists
        var nicknameTakenErrors = await userService.NicknameAlreadyRegistered(
            request.Username,
            request.Email,
            cancellationToken,
            excludedId: user.Uuid);

        if (nicknameTakenErrors is not null)
            return ErrorResults.Conflict("The user already exists", nicknameTakenErrors);
        
        if(request.Password is not null)
        {
            var newPasswordHash = passwordHashService.GetPasswordHash(request.Password);
            user.PasswordHash = newPasswordHash;
        }

        user.Username = request.Username;
        user.Email = request.Email;

        db.Update(user);    
        await db.SaveChangesAsync(cancellationToken);

        var response = new UserResponse(
            user.Uuid,
            user.CreatedAt,
            user.UpdatedAt,
            user.Username,
            user.Email,
            user.Elo.Rating,
            user.WinAmount,
            user.DrawAmount,
            user.LossAmount
        );

        return Results.Ok(response);
    }
}