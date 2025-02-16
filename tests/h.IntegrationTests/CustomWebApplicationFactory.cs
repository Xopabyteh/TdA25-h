using h.Contracts.Users;
using h.Server.Features.Matchmaking;
using h.Server.Infrastructure.Database;
using h.Server.Infrastructure.Matchmaking;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(DbContextOptions<AppDbContext>))!;

            services.Remove(dbContextDescriptor);

            var dbConnectionDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(DbConnection))!;

            services.Remove(dbConnectionDescriptor);

            // Create open SqliteConnection so EF won't automatically close it.
            services.AddSingleton<DbConnection>(container =>
            {
                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();

                return connection;
            });

            services.AddDbContext<AppDbContext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
            });

            // Remove matchmaking background service and turn it into a singleton
            services.Remove(
                services.Single(s => s.ImplementationType == typeof(MatchPlayersBackgroundService))
            );
            services.AddSingleton<MatchPlayersBackgroundService>();
        });

        builder.UseEnvironment("Development");
    }

    /// <summary>
    /// Creates a new user and logs into him.
    /// Make sure the test depends on <see cref="h.IntegrationTests.Auth.AuthTests.Login_ValidUser_ReturnsSuccess"/>.
    /// </summary>
    /// <returns>HttpClient with authorization header</returns>
    public async Task<HttpClient> CreateUserAndLoginAsync(
        string nickname,
        ulong eloRating)
    {
        var client = CreateClient();
        var request = new CreateNewUserRequest(
            nickname,
            $"{nickname}@tda.h",
            "P@ssw0rd",
            eloRating
        );

        // Create user
        await client.PostAsJsonAsync("/api/v1/users", request);

        var loginRequest = new LoginUserRequest(
            nickname,
            "P@ssw0rd"
        );

        // Login user
        var loginResponse = await client.PostAsJsonAsync("/api/v1/users/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthenticationResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            loginResult.Token);

        return client;
    }

    public async Task<HubConnection> CreateSignalRConnection(string hubName, string? jwtToken = null)
    {
        var handler = Server.CreateHandler();
	    var hubConnection = new HubConnectionBuilder()
		    .WithUrl($"wss://localhost/{hubName}", o =>
		    {
			    o.HttpMessageHandlerFactory = _ => handler;
                // Using JWT bearer token
                o.AccessTokenProvider = () => Task.FromResult(jwtToken);
            })
		    .Build();
 
	    return hubConnection;
    }
}