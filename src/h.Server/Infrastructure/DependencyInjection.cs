using Carter;
using FluentValidation;
using h.Client.Services;
using h.Client.Services.Game;
using h.Primitives.Games;
using h.Server.Infrastructure.Auth;
using h.Server.Infrastructure.Database;
using h.Server.Infrastructure.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;
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
                .RequireAuthenticatedUser().Build());
        });
        builder.Services.AddScoped<UserService>();
        builder.Services.AddScoped<PasswordHashService>();
        builder.Services.AddScoped<JwtTokenService>();

        return builder;
    }
}
