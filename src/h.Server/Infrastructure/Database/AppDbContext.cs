using h.Primitives.Users;
using h.Server.Entities.Games;
using h.Server.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartEnum.EFCore;

namespace h.Server.Infrastructure.Database;

public class AppDbContext : DbContext
{
    private readonly IConfiguration _config;
    public AppDbContext(DbContextOptions options, IConfiguration config) : base(options)
    {
        // DI, NOOP
        _config = config;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    
        optionsBuilder.UseAsyncSeeding(async (context, _, cancellationToken) =>
        {
            var passwordHasher = new PasswordHasher<object>();
            var adminUser = await context.Set<User>().FirstOrDefaultAsync(u => u.Email == _config["Auth:AdminUser:Email"]);
            
            if(adminUser is null)
            {
                var admin = new User
                {
                    Email = _config["Auth:AdminUser:Email"]!,
                    Username = _config["Auth:AdminUser:Username"]!,
                    PasswordHash = passwordHasher.HashPassword(null!, _config["Auth:AdminUser:Password"]!),
                    Elo = new(),
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
}
