using Carter;
using FluentValidation;
using h.Client.Services;
using h.Client.Services.Game;
using h.Contracts.Components.Services;
using h.Primitives.Games;
using h.Server.Components.Services;
using h.Server.Infrastructure.Database;
using h.Server.Infrastructure.Middleware;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

        builder.Services.AddTransient<ToastService>();

        // Add dummy implementations for server-side services (used when prerendering)
        builder.Services.AddScoped<IWasmHttpClient, Server.Components.Services.WasmHttpClient>();
        builder.Services.AddSingleton<IWasmGameService, Server.Components.Services.WasmGameService>();

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
