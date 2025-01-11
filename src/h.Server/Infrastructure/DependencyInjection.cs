using Carter;
using FluentValidation;
using h.Contracts.Components.Services;
using h.Primitives.Games;
using h.Server.Components.Services;
using h.Server.Infrastructure.Database;
using h.Server.Infrastructure.Middleware;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Infrastructure;
public static class DependencyInjection
{
    public static WebApplicationBuilder AddPresentation(this WebApplicationBuilder builder)
    {
        builder.AddBlazorComponents();

        builder.Services.AddCarter();
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        builder.Services.AddScoped<BadRequestResponseMiddleware>();

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

    private static WebApplicationBuilder AddBlazorComponents(this WebApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        builder.Services.AddScoped<IWasmOnlyHttpClient, WasmOnlyHttpClient>();
    
        return builder;
    }

    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);

        // Add EF Core
        builder.Services.AddScoped<AutoSetUpdatedAtDbSaveInterceptor>();
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(
                builder.Configuration.GetConnectionString("Database")
            );

            // Add interceptors
            var serviceProvider = builder.Services.BuildServiceProvider();
            var autoSetUpdatedAtInterceptor = serviceProvider.GetRequiredService<AutoSetUpdatedAtDbSaveInterceptor>();
            options.AddInterceptors(autoSetUpdatedAtInterceptor);
        });

        return builder;
    }
}
