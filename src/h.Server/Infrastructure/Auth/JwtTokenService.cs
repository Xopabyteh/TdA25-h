using h.Server.Entities.Users;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace h.Server.Infrastructure.Auth;

public class JwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateTokenFor(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Auth:Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claimsFromRoles = user.Roles
            .Select(role => new Claim(ClaimTypes.Role, Enum.GetName(role)!))
            .ToArray();

        var Sectoken = new JwtSecurityToken(
            issuer: _config["Auth:Jwt:Issuer"]!,
            audience: _config["Auth:Jwt:Audience"],
            claims: [
                // Claims from user
                new Claim(ClaimTypes.NameIdentifier, user.Uuid.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                
                // Roles
                ..claimsFromRoles
            ],
            expires: DateTime.Now.AddMinutes(_config.GetValue<double>("Auth:Jwt:ExpireInMinutes")),
            signingCredentials: credentials);

        var token =  new JwtSecurityTokenHandler().WriteToken(Sectoken);

        return token;
    }
}
