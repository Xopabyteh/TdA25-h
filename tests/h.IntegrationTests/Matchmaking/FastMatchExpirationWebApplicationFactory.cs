using h.Contracts.Matchmaking;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace h.Server.Infrastructure.Matchmaking;

public class FastMatchExpirationWebApplicationFactory : CustomWebApplicationFactory
{
    public const int FastMatchExpirationSeconds = 1;
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Override macthmaking options to make match expire very fast
        builder.ConfigureServices(services =>
        {
            services.Configure<MatchmakingOptions>(c =>
            {
                c.MatchingExpiresInSeconds = FastMatchExpirationSeconds;
                c.PlayerHasToAcceptInSeconds = FastMatchExpirationSeconds;
            });
        });

        base.ConfigureWebHost(builder);
    }

    public class MethodDataSource
    {
        public static Func<FastMatchExpirationWebApplicationFactory> Get()
            => () => new FastMatchExpirationWebApplicationFactory();
    }
}
