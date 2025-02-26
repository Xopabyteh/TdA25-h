using Carter;
using h.Contracts;
using h.Contracts.Users;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Auth;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Features.Users;

public static class AdminUpdateUser
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/v1/users/admin-update/{id}", Handle)
                .RequireAuthorization(nameof(AppPolicies.IsAdmin));
        }
    }

    public static async Task<IResult> Handle(
        [FromServices] AppDbContext db,
        [FromServices] PasswordHashService passwordHashService,
        [FromServices] UserService userService,
        [FromRoute] Guid id,
        [FromBody] AdminUpdateUserRequest request,
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
            return ErrorResults.Conflict("The user already exists", nicknameTakenErrors);
        
        if(request.Password is not null)
        {
            var newPasswordHash = passwordHashService.GetPasswordHash(request.Password);
            user.PasswordHash = newPasswordHash;
        }

        user.Username = request.Username;
        user.Email = request.Email;
        user.WinAmount = request.WinAmount;
        user.DrawAmount = request.DrawAmount;
        user.LossAmount = request.LossAmount;
        user.Elo = new()
        {
            Rating = request.Elo
        };

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
            user.LossAmount,
            user.BannedFromRankedMatchmakingAt  
        );

        return Results.Ok(response);
    }
}