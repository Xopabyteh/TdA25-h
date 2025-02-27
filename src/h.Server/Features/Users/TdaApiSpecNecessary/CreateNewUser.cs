﻿using Carter;
using h.Contracts.Users;
using h.Server.Infrastructure.Auth;
using h.Server.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using h.Server.Infrastructure.Database;
using FluentValidation;
using h.Server.Entities.Users;

namespace h.Server.Features.Users.TdaApiSpecNecessary;

public static class CreateNewUser
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/users", Handle);
        }
    }

    public static async Task<IResult> Handle(
        [FromServices] AppDbContext db,
        [FromServices] UserService userService,
        [FromServices] PasswordHashService passwordHashService,
        [FromServices] IValidator<CreateNewUserRequest> validator,
        CreateNewUserRequest request,
        CancellationToken cancellationToken)
    {
        // Validate
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
            return ErrorResults.ValidationProblem(validationResult);

        // Check if user exists
        var nicknameTakenErrors = await userService.NicknameAlreadyRegistered(request.Username, request.Email, cancellationToken);
        if (nicknameTakenErrors is not null)
            return ErrorResults.Conflict("The user already exists", nicknameTakenErrors);
        
        // Hash password
        var passwordHash = passwordHashService.GetPasswordHash(request.Password);

        // Create user
        var user = User.NewUser(request.Username, request.Email, passwordHash, request.Elo);

        await db.UsersDbSet.AddAsync(user, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Map and return
        return Results.Created(
            $"/api/v1/users/{user.Uuid}",
            new UserResponse(
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
            )
        );
    }

    public class Validator : AbstractValidator<CreateNewUserRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Password).SetValidator(new SharedPasswordValidator());
            RuleFor(x => x.Username).SetValidator(new SharedUsernameValidator());

            RuleFor(x => x.Email).NotEmpty();
        }
    }
}
