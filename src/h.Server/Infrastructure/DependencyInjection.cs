using Carter;
using h.Server.Entities.Games;
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
            o.SerializerOptions.Converters.Add(new GameDifficultyJsonConverter());
        });
        
        return builder;
    }

    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        // Add efcore with sqlite database
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("Database"));
        });

        return builder;
    }
}
