using Carter;
using h.Contracts.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

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
        LoginRequest request)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Auth:Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var Sectoken = new JwtSecurityToken(
            issuer: config["Auth:Jwt:Issuer"]!,
            audience: config["Auth:Jwt:Audience"],
            claims: [],
            expires: DateTime.Now.AddMinutes(config.GetValue<double>("Auth:Jwt:ExpireInMinutes")),
            signingCredentials: credentials);

        var token =  new JwtSecurityTokenHandler().WriteToken(Sectoken);

        return Results.Ok(token);
    }
}
