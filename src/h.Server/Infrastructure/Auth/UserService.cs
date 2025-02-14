using ErrorOr;
using h.Contracts;
using h.Server.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Infrastructure.Auth;

public class UserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Nickname being an alias for either username or email.
    /// Returns a list of errors if the nickname is already registered.
    /// Null if the nickname is not registered.
    /// </summary>
    /// <param name="excludedId">Optional. If provided, the user with this id will be excluded from the search.</param>
    public async Task<List<Error>?> NicknameAlreadyRegistered(
        string username,
        string email,
        CancellationToken cancellationToken,
        Guid? excludedId = null)
    {
        var userWithNewUsername = await _db.UsersDbSet
            .Where(u => excludedId == null || u.Uuid != excludedId)
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        
        var userWithNewEmail = await _db.UsersDbSet
            .Where(u => excludedId == null || u.Uuid != excludedId)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        
        if (userWithNewUsername is not null && userWithNewEmail is not null)
            return new List<Error> { SharedErrors.User.UsernameAlreadyTaken(), SharedErrors.User.EmailAlreadyTaken() };
        if (userWithNewUsername is not null)
            return new List<Error> { SharedErrors.User.UsernameAlreadyTaken() };
        if (userWithNewEmail is not null)
            return new List<Error> { SharedErrors.User.EmailAlreadyTaken() };
        
        return null;
    }
}
