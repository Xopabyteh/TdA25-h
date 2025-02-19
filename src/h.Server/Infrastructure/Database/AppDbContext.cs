using h.Primitives.Users;
using h.Server.Entities.Games;
using h.Server.Entities.MultiplayerGames;
using h.Server.Entities.Users;
using h.Server.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using SmartEnum.EFCore;

namespace h.Server.Infrastructure.Database;

/// <summary>
/// Responsible for db context configuration and seeding.
/// </summary>
public class AppDbContext : DbContext
{
    private readonly IConfiguration _config;
    private readonly PasswordHashService passwordHashService;
    public AppDbContext(DbContextOptions options, IConfiguration config, PasswordHashService passwordHashService) : base(options)
    {
        // DI, NOOP
        _config = config;
        this.passwordHashService = passwordHashService;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    
        optionsBuilder.UseAsyncSeeding(async (context, _, cancellationToken) =>
        {
            var adminUser = await context.Set<User>().FirstOrDefaultAsync(u => u.Email == _config["Auth:AdminUser:Email"]);
            
            if(adminUser is null)
            {
                var admin = new User
                {
                    Email = _config["Auth:AdminUser:Email"]!,
                    Username = _config["Auth:AdminUser:Username"]!,
                    PasswordHash = passwordHashService.GetPasswordHash(_config["Auth:AdminUser:Password"]!),
                    Elo = new(0),
                    WinAmount = 0,
                    LossAmount = 0,
                    DrawAmount = 0,
                    Roles = [
                        UserRole.Admin
                    ]
                };
                
                context.Set<User>().Add(admin);
                await context.SaveChangesAsync(cancellationToken);
            }
        });
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);  
        
        configurationBuilder.ConfigureSmartEnum();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureSmartEnum();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public DbSet<Game> GamesDbSet { get; set; }
    public DbSet<User> UsersDbSet { get; set; }
    public DbSet<UserToFinishedRankedGame> UserToFinishedRankedGames { get; set; }
    public DbSet<FinishedRankedGame> FinishedRankedGames { get; set; }
}
