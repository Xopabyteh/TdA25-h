using Microsoft.AspNetCore.Identity;

namespace h.Server.Infrastructure.Auth;

public class PasswordHashService
{
    public string GetPasswordHash(string password)
    {
        var passwordHasher = new PasswordHasher<object>();
        var hash = passwordHasher.HashPassword(null!, password);
    
        return hash;
    }

    public PasswordVerificationResult VerifyHashedPassword(string passwordHash, string password)
    {
        var passwordHasher = new PasswordHasher<object>();
        var result = passwordHasher.VerifyHashedPassword(null!, passwordHash, password);

        return result;
    }
}
