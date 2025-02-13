using Carter;
using FluentValidation;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Auth;
using h.Contracts.Users;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using h.Contracts;
using h.Server.Entities.Users;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace h.Server.Features.Users;

public static class RegisterUser
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/users/register", Handle);
        }
    }
    public static async Task<IResult> Handle(
        [FromServices] IConfiguration config,
        [FromServices] AppDbContext db,
        [FromServices] IValidator<RegisterUserRequest> validator,
        [FromServices] JwtTokenService tokenService,
        [FromServices] IAuthenticationService authenticationService,
        HttpContext httpContext,
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        // Validate
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
            return ErrorResults.ValidationError(validationResult);

        // Check if user exists
        var userExists = await db.UsersDbSet.AnyAsync(u => u.Email == request.Email || u.Username == request.Username, cancellationToken);
        if (userExists)
            return ErrorResults.Conflit("The user already exists", [SharedErrors.User.UserAlreadyExists()]);
        
        // Hash password
        var passwordHasher = new PasswordHasher<object>();
        var passwordEncrypted = passwordHasher.HashPassword(null!, request.Password);

        // Create user
        var user = User.NewUser(request.Username, request.Email, passwordEncrypted);

        await db.UsersDbSet.AddAsync(user, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        
        // Generate token
        var token = tokenService.GenerateTokenFor(user);

        // Add token to response
        httpContext.Response.Headers.Append("Authorization", $"Bearer {token}");

        // Map and return
        return Results.Ok(
            new AuthenticatiionResponse(
                token,
                new UserResponse(
                    user.Uuid,
                    user.CreatedAt,
                    user.UpdatedAt,
                    user.Username,
                    user.Email,
                    user.Elo.Rating,
                    user.WinAmount,
                    user.DrawAmount,
                    user.LossAmount
                )
            )
        );
    }

    public class Validator : AbstractValidator<RegisterUserRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Password).NotEmpty();
            RuleFor(x => x.Email).NotEmpty();
            RuleFor(x => x.Username).NotEmpty();
        }
    }
}