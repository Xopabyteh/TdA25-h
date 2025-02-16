using Carter;
using FluentValidation;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Auth;
using h.Contracts.Users;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using h.Server.Entities.Users;
using Microsoft.AspNetCore.Authentication;

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
        [FromServices] UserService userService,
        [FromServices] PasswordHashService passwordHashService,
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
        var nicknameTakenErrors = await userService.NicknameAlreadyRegistered(request.Username, request.Email, cancellationToken);
        if (nicknameTakenErrors is not null)
            return ErrorResults.Conflit("The user already exists", nicknameTakenErrors);
        
        // Hash password
        var passwordHash = passwordHashService.GetPasswordHash(request.Password);

        // Create user
        var user = User.NewUser(request.Username, request.Email, passwordHash);

        await db.UsersDbSet.AddAsync(user, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        
        // Generate token
        var token = tokenService.GenerateTokenFor(user);

        // Add token to response
        httpContext.Response.Headers.Append("Authorization", $"Bearer {token}");

        // Map and return
        return Results.Ok(
            new AuthenticationResponse(
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

            // Todo: Validate password specs
        }
    }
}