using Carter;
using FluentValidation;
using h.Client.Services;
using h.Client.Services.Game;
using h.Contracts.Matchmaking;
using h.Primitives.Games;
using h.Server.Features.Matchmaking;
using h.Server.Infrastructure.AuditLog;
using h.Server.Infrastructure.Auth;
using h.Server.Infrastructure.Database;
using h.Server.Infrastructure.GameInvitations;
using h.Server.Infrastructure.Matchmaking;
using h.Server.Infrastructure.Middleware;
using h.Server.Infrastructure.MultiplayerGames;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using h.Server.Components.Services;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Authentication.Cookies;
using h.Contracts;
using h.Client.Pages;
using h.Server.Infrastructure.Leaderboard;
using System.Threading.Channels;

namespace h.Server.Infrastructure;
public static class DependencyInjection
{
    public static WebApplicationBuilder AddPresentation(this WebApplicationBuilder builder)
    {
        builder.AddBlazorComponents();

        builder.Services.AddCarter();
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        builder.Services.AddScoped<BadRequestResponseMiddleware>();
        builder.Services.AddScoped<GuestLoginEnsureMiddleware>();

        // Add custom json converters
        // Mainly for custom model bindings
        builder.Services.Configure<JsonOptions>(o =>
        {
            // Primitives parsers
            o.SerializerOptions.Converters.Add(new GameDifficultyJsonConverter());
            o.SerializerOptions.Converters.Add(new GameStateJsonConverter());
            o.SerializerOptions.Converters.Add(new ErrorConverter());
        });

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddCors();

        return builder;
    }

    private static WebApplicationBuilder AddBlazorComponents(this WebApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents()
            .AddAuthenticationStateSerialization(o =>
            {
                o.SerializeAllClaims = true;
            }); 

        builder.Services.AddCascadingAuthenticationState();

        builder.Services.AddBlazoredSessionStorage();
        //builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

        builder.Services.AddTransient<ToastService>();

        // Add dummy implementations for server-side services (used when prerendering)
        builder.Services.AddScoped<IWasmHttpClient, Server.Components.Services.WasmHttpClient>();
        builder.Services.AddSingleton<IWasmGameService, Server.Components.Services.WasmGameService>();
        builder.Services.AddNullService<IHApiClient>();
        builder.Services.AddNullService<IWasmCurrentUserStateService>();

        return builder;
    }

    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        // Core
        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);

        // SignalR
        builder.Services.AddSignalR();

        builder.Services.AddSingleton(typeof(IHubUserIdMappingService<>), typeof(InMemoryHubUserIdMappingService<>));
        builder.Services.AddSingleton(typeof(IHubUserIdMappingService<,>), typeof(InMemoryHubUserIdMappingService<,>));

        // Add EF Core (db context & db context factory)
        builder.Services.AddScoped<AutoSetUpdatedAtDbSaveInterceptor>();
        Action<DbContextOptionsBuilder>? dbOptionsAction = options =>
                {
                    options.UseSqlite(builder.Configuration.GetConnectionString("Database"));

                    // Add interceptors
                    var serviceProvider = builder.Services.BuildServiceProvider();
                    var autoSetUpdatedAtInterceptor = serviceProvider.GetRequiredService<AutoSetUpdatedAtDbSaveInterceptor>();
                    options.AddInterceptors(autoSetUpdatedAtInterceptor);
                };
        builder.Services
            .AddDbContextFactory<AppDbContext>(dbOptionsAction, lifetime: ServiceLifetime.Singleton)
            .AddDbContext<AppDbContext>(dbOptionsAction, optionsLifetime: ServiceLifetime.Singleton);


        // Auth
        builder.Services
            .AddAuthentication(o =>
            {
                o.DefaultScheme = "HybridAuth"; // Custom scheme that tries both
                o.DefaultAuthenticateScheme = "HybridAuth";
                o.DefaultChallengeScheme = "HybridAuth";
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
	            //jwtOptions.Authority = builder.Configuration["Auth:Jwt:Authority"];
	            options.Audience = builder.Configuration["Auth:Jwt:Audience"];
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = false,
		            ValidateAudience = true,
		            ValidateIssuerSigningKey = true,
		            ValidAudiences = builder.Configuration.GetSection("Auth:Jwt:ValidAudiences").Get<string[]>(),
		            //ValidIssuers = builder.Configuration.GetSection("Auth:Jwt:ValidIssuers").Get<string[]>(),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Auth:Jwt:Key"]!))
                };
            })
             .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = "h.Auth";
                options.LoginPath = PageRoutes.Auth.LoginIndex;
                options.LogoutPath = PageRoutes.Auth.Logout;

                options.AccessDeniedPath = PageRoutes.HomeIndex; // Redirect to home if access denied
                
                options.ReturnUrlParameter = "r";
            })
             .AddPolicyScheme("HybridAuth", JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    var authHeader = context.Request.Headers["Authorization"];
                    if (!string.IsNullOrEmpty(authHeader))
                    {
                        return JwtBearerDefaults.AuthenticationScheme; // Use JWT if present
                    }
                    return CookieAuthenticationDefaults.AuthenticationScheme; // Otherwise, use cookies
                };
            });
        
        builder.Services.AddAuthorization(o => o.AddAppPolicies());

        builder.Services.AddScoped<UserService>();
        builder.Services.AddSingleton<PasswordHashService>();
        builder.Services.AddSingleton<JwtTokenCreationService>();
        builder.Services.AddSingleton<AppIdentityCreationService>();
        builder.Services.AddSingleton<GuestLoginService>();

        // Matchmaking
        builder.Services.AddSingleton<IMatchmakingQueueService, InMemoryMatchmakingQueueService>();
        builder.Services.AddSingleton<InMemoryMatchmakingService>();
        builder.Services.AddHostedService<MatchPlayersBackgroundService>();
        builder.Services.AddHostedService<RemoveExpiredMatchingsBackgroundService>();
        builder.Services.Configure<MatchmakingOptions>(builder.Configuration.GetSection(MatchmakingOptions.SectionName));

        // MultiplayerGames
        builder.Services.AddSingleton<IMultiplayerGameSessionService, InMemoryMultiplayerGameSessionService>();
        builder.Services.AddScoped<MultiplayerGameStatisticsService>();  

        // GameInvitations
        builder.Services.AddSingleton<InMemoryInvitationCodeService>();

        // Audit log
        builder.Services.AddScoped<AuditLogService>();

        // Leaderboard
        builder.Services.AddScoped<LeaderboardService>();   

        return builder;
    }
}
