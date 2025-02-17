using Carter;
using FluentValidation;
using h.Client.Services;
using h.Client.Services.Game;
using h.Contracts.Matchmaking;
using h.Primitives.Games;
using h.Primitives.Users;
using h.Server.Features.Matchmaking;
using h.Server.Infrastructure.Auth;
using h.Server.Infrastructure.Database;
using h.Server.Infrastructure.Matchmaking;
using h.Server.Infrastructure.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

        builder.Services.AddCors();

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
        // Core
        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);

        // SignalR
        builder.Services.AddSignalR();
        builder.Services.AddSingleton(typeof(IHubUserIdMappingService<>), typeof(InMemoryHubUserIdMappingService<>));

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

        // Auth
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwtOptions =>
            {
	            //jwtOptions.Authority = builder.Configuration["Auth:Jwt:Authority"];
	            jwtOptions.Audience = builder.Configuration["Auth:Jwt:Audience"];
                jwtOptions.TokenValidationParameters = new()
                {
                    ValidateIssuer = false,
		            ValidateAudience = true,
		            ValidateIssuerSigningKey = true,
		            ValidAudiences = builder.Configuration.GetSection("Auth:Jwt:ValidAudiences").Get<string[]>(),
		            //ValidIssuers = builder.Configuration.GetSection("Auth:Jwt:ValidIssuers").Get<string[]>(),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Auth:Jwt:Key"]!))
                };
            });
        
        builder.Services.AddAuthorization(c =>
        {
            c.AddPolicy(JwtBearerDefaults.AuthenticationScheme, new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
                .RequireAuthenticatedUser()
                .Build()
            );

            c.AddPolicy(AppPolicyNames.AbleToJoinMatchmaking, pb =>
            {
                pb.RequireAuthenticatedUser();
                
                // Assert the user is not an admin
                pb.RequireAssertion(context =>
                {
                    var user = context.User;
                    return !user.IsInRole(nameof(UserRole.Admin));
                });
            });
        });
        builder.Services.AddScoped<UserService>();
        builder.Services.AddScoped<PasswordHashService>();
        builder.Services.AddScoped<JwtTokenService>();

        // Matchmaking
        builder.Services.AddSingleton<IMatchmakingQueueService, InMemoryMatchmakingQueueService>();
        builder.Services.AddSingleton<InMemoryMatchmakingService>();
        builder.Services.AddHostedService<MatchPlayersBackgroundService>();
        builder.Services.AddHostedService<RemoveExpiredMatchingsBackgroundService>();
        builder.Services.Configure<MatchmakingOptions>(builder.Configuration.GetSection(MatchmakingOptions.SectionName));

        return builder;
    }
}
