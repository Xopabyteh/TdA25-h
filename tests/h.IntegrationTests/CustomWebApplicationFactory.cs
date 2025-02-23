using h.Contracts.Users;
using h.Server.Features.Matchmaking;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Data.Common;
using System.Net.Http.Headers;
using System.Net.Http.Json;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Re-register AppDbContext to use in-memory database
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(DbContextOptions<AppDbContext>))!;
            services.Remove(dbContextDescriptor);
            var dbConnectionDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(DbConnection))!;
            services.Remove(dbConnectionDescriptor);

            // Create open SqliteConnection so EF won't automatically close it.
            services.AddSingleton((Func<IServiceProvider, DbConnection>)(container =>
            {
                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();

                return connection;
            }));

            Action<IServiceProvider, DbContextOptionsBuilder>? dbOptionsAction = (container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
            };
            
            services
                .AddDbContextFactory<AppDbContext>(dbOptionsAction, lifetime: ServiceLifetime.Singleton)
                .AddDbContext<AppDbContext>(dbOptionsAction, optionsLifetime: ServiceLifetime.Singleton);

            // Remove matchmaking background service and turn it into a singleton
            services.Remove(
                services.Single(s => s.ImplementationType == typeof(MatchPlayersBackgroundService))
            );
            services.AddSingleton<MatchPlayersBackgroundService>();

            // Remove matchmaking expiration background service and turn it into a singleton
            services.Remove(
                services.Single(s => s.ImplementationType == typeof(RemoveExpiredMatchingsBackgroundService))
            );
            services.AddSingleton<RemoveExpiredMatchingsBackgroundService>();
        });

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(
            [
                new("Auth:AdminUser:Username", "TdA"),
                new("Auth:AdminUser:Email", "tda@sgc.cz"),
                new("Auth:AdminUser:Password", "P@ssw0rd-admin")
            ]);
        });

        builder.UseEnvironment("Development");

        base.ConfigureWebHost(builder);
    }

    private static int lastNicknameNumber = 0;
    private static string NewIncrementalNickname()
    {
        Interlocked.Increment(ref lastNicknameNumber);
        return $"user{lastNicknameNumber}";
    }

    /// <summary>
    /// Creates a new user and logs into him.
    /// Make sure the test depends on <see cref="h.IntegrationTests.Auth.AuthTests.Login_ValidUser_ReturnsSuccess"/>.
    /// Don't forget to dispose the client after the test.
    /// </summary>
    /// <returns>HttpClient with authorization header</returns>
    public async Task<(HttpClient client, AuthenticationResponse loginResult)> CreateUserAndLoginAsync(
        int eloRating)
    {
        var name = NewIncrementalNickname();
        var client = CreateClient();
        var request = new CreateNewUserRequest(
            name,
            $"{name}@tda.h",
            "P@ssw0rd",
            eloRating
        );

        // Create user
        var response = await client.PostAsJsonAsync("/api/v1/users", request);
        response.EnsureSuccessStatusCode();

        var loginRequest = new LoginUserRequest(
            name,
            "P@ssw0rd"
        );

        // Login user
        var loginResponse = await client.PostAsJsonAsync("/api/v1/users/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthenticationResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            loginResult.Token);

        return (client, loginResult);
    }

    /// <summary>
    /// Logs into the admin user.
    /// Don't forget to dispose the client after the test.
    /// </summary>
    public async Task<(HttpClient client, AuthenticationResponse loginResult)> LoginAdminAsync()
    {
        var client = CreateClient();
        var loginRequest = new LoginUserRequest(
            "TdA",
            "P@ssw0rd-admin"
        );

        // Login user
        var loginResponse = await client.PostAsJsonAsync("/api/v1/users/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthenticationResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            loginResult.Token);

        return (client, loginResult);
    }

    /// <summary>
    /// Logs into a guest user.
    /// Don't forget to dispose the client after the test.
    /// </summary>
    public async Task<(HttpClient client, GuestLoginResponse auth)> LoginGuestAsync()
    {
        var client = CreateClient();

        // Login user
        var loginResponse = await client.PostAsync("/api/v1/users/guest-login", null);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<GuestLoginResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            loginResult.Token);
        
        return (client, loginResult);
    }

    public HubConnection CreateSignalRConnection(string hubName, string? jwtToken = null)
    {
        var handler = Server.CreateHandler();
	    var hubConnection = new HubConnectionBuilder()
		    .WithUrl($"ws://localhost/{hubName}", o =>
		    {
                o.HttpMessageHandlerFactory = _ => handler;
                // Using JWT bearer token
                o.AccessTokenProvider = () => Task.FromResult(jwtToken);
            })
		    .Build();
 
	    return hubConnection;
    }
}