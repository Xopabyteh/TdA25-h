using Carter;
using FluentValidation;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LoginRequest = h.Contracts.Users.LoginRequest;

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
        [FromServices] IValidator<LoginRequest> validator,
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        // Validate
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
            return ErrorResults.ValidationError(validationResult);

        var user = await db.UsersDbSet.FirstOrDefaultAsync(u => u.Email == request.Nickname, cancellationToken)
            ?? await db.UsersDbSet.FirstOrDefaultAsync(u => u.Username == request.Nickname, cancellationToken);

        if(user is null)
            return Results.Unauthorized();

        var passwordHasher = new PasswordHasher<object>();
        var result = passwordHasher.VerifyHashedPassword(null!, user.PasswordEncrypted, request.Password);
        
        if(result is PasswordVerificationResult.Failed)
            return Results.Unauthorized();

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Auth:Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var Sectoken = new JwtSecurityToken(
            issuer: config["Auth:Jwt:Issuer"]!,
            audience: config["Auth:Jwt:Audience"],
            claims: [
                new Claim(JwtRegisteredClaimNames.Sub, user.Uuid.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
            ],
            expires: DateTime.Now.AddMinutes(config.GetValue<double>("Auth:Jwt:ExpireInMinutes")),
            signingCredentials: credentials);

        var token =  new JwtSecurityTokenHandler().WriteToken(Sectoken);

        return Results.Ok(token);
    }

    public class Validator : AbstractValidator<LoginRequest>
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
