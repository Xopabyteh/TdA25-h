using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace h.Server.Infrastructure.Auth;

public class JwtTokenCreationService
{
    private readonly IConfiguration _config;

    public JwtTokenCreationService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(IReadOnlyCollection<Claim> withClaims)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Auth:Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var Sectoken = new JwtSecurityToken(
            issuer: _config["Auth:Jwt:Issuer"]!,
            audience: _config["Auth:Jwt:Audience"],
            claims: withClaims,
            expires: DateTime.Now.AddMinutes(_config.GetValue<double>("Auth:Jwt:ExpireInMinutes")),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(Sectoken);
    }
}
