using Carter;
using FluentValidation;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Auth;
using h.Contracts.Users;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using h.Server.Entities.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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
        [FromBody]RegisterUserRequest request,
        [FromServices] IConfiguration config,
        [FromServices] AppDbContext db,
        [FromServices] UserService userService,
        [FromServices] PasswordHashService passwordHashService,
        [FromServices] IValidator<RegisterUserRequest> validator,
        [FromServices] JwtTokenCreationService tokenService,
        [FromServices] IAuthenticationService authenticationService,
        [FromServices] AppIdentityCreationService identityService,
        HttpContext httpContext,
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
        var user = User.NewUser(request.Username, request.Email, passwordHash);

        await db.UsersDbSet.AddAsync(user, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Create identity
        var claims = identityService.GetClaimsForUser(user);

        // Generate token
        var token = tokenService.GenerateToken(claims);

        // Add token to response
        httpContext.Response.Headers.Append("Authorization", $"Bearer {token}");

        // Sign in with Cookie
        var principal = identityService.GeneratePrincipalFromClaims(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true });

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
                    user.LossAmount,
                    user.BannedFromRankedMatchmakingAt
                )
            )
        );
    }

    public class Validator : AbstractValidator<RegisterUserRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Password).SetValidator(new SharedPasswordValidator());
            RuleFor(x => x.Username).SetValidator(new SharedUsernameValidator());

            RuleFor(x => x.Email).NotEmpty();
        }
    }
}