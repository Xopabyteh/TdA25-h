﻿using Carter;
using FluentValidation;
using h.Contracts.Users;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Auth;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        [FromBody] LoginUserRequest request,
        [FromServices] IConfiguration config,
        [FromServices] AppDbContext db,
        [FromServices] PasswordHashService passwordHashService,
        [FromServices] IValidator<LoginUserRequest> validator,
        [FromServices] JwtTokenCreationService tokenService,
        [FromServices] AppIdentityCreationService identityService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        // Validate
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
            return ErrorResults.ValidationProblem(validationResult);

        // Try get user
        var user = await db.UsersDbSet.FirstOrDefaultAsync(u => u.Email == request.Nickname, cancellationToken)
            ?? await db.UsersDbSet.FirstOrDefaultAsync(u => u.Username == request.Nickname, cancellationToken);

        if(user is null)
            return Results.Unauthorized();

        // Verify password hash
        var result = passwordHashService.VerifyHashedPassword(user.PasswordHash, request.Password);
        
        if(result is PasswordVerificationResult.Failed)
            return Results.Unauthorized();

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
        return Results.Ok(new AuthenticationResponse(
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