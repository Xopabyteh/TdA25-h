using Carter;
using FluentValidation;
using h.Contracts.Users;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Auth;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LoginUserRequest = h.Contracts.Users.LoginUserRequest;

namespace h.Server.Features.Users;

public static class LoginUser
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/users/login", Handle);
        }
    }
    public static async Task<IResult> Handle(
        [FromServices] IConfiguration config,
        [FromServices] AppDbContext db,
        [FromServices] IValidator<LoginUserRequest> validator,
        [FromServices] JwtTokenService tokenService,
        [FromServices] IAuthenticationService authenticationService,
        HttpContext httpContext,
        LoginUserRequest request,
        CancellationToken cancellationToken)
    {
        // Validate
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
            return ErrorResults.ValidationError(validationResult);

        // Try get user
        var user = await db.UsersDbSet.FirstOrDefaultAsync(u => u.Email == request.Nickname, cancellationToken)
            ?? await db.UsersDbSet.FirstOrDefaultAsync(u => u.Username == request.Nickname, cancellationToken);

        if(user is null)
            return Results.Unauthorized();

        // Verify password hash
        var passwordHasher = new PasswordHasher<object>();
        var result = passwordHasher.VerifyHashedPassword(null!, user.PasswordHash, request.Password);
        
        if(result is PasswordVerificationResult.Failed)
            return Results.Unauthorized();

        // Generate token
        var token = tokenService.GenerateTokenFor(user);

        // Add token to response
        httpContext.Response.Headers.Append("Authorization", $"Bearer {token}");

        // Map and return
        return Results.Ok(new AuthenticatiionResponse(
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
        ));
    }

    public class Validator : AbstractValidator<LoginUserRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Password).NotEmpty();

            RuleFor(x => x.Nickname)
                .NotEmpty()
                .WithMessage("Username or Email is required.");
        }
    }
}