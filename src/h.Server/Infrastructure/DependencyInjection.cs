using Carter;
using h.Primitives.Games;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Infrastructure;
public static class DependencyInjection
{
    public static WebApplicationBuilder AddPresentation(this WebApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        builder.Services.AddCarter();

        // Add custom json converters
        // Mainly for custom model bindings
        builder.Services.Configure<JsonOptions>(o =>
        {
            // Primitives parsers
            o.SerializerOptions.Converters.Add(new GameDifficultyJsonConverter());
            o.SerializerOptions.Converters.Add(new GameStateJsonConverter());
        });
        
        return builder;
    }

    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);

        // Add EF Core
        builder.Services.AddScoped<AutoSetUpdatedAtDbSaveInterceptor>();
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("Database"));
            
            // Add interceptors
            var serviceProvider = builder.Services.BuildServiceProvider();
            var autoSetUpdatedAtInterceptor = serviceProvider.GetRequiredService<AutoSetUpdatedAtDbSaveInterceptor>();
            options.AddInterceptors(autoSetUpdatedAtInterceptor);
        });

        return builder;
    }
}
