using Carter;
using h.Contracts;
using h.Contracts.GameInvitations;
using h.Contracts.Matchmaking;
using h.Contracts.MultiplayerGames;
using h.Server.Components;
using h.Server.Features.GameInvitations;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Matchmaking;
using h.Server.Infrastructure.Middleware;
using h.Server.Infrastructure.MultiplayerGames;

var builder = WebApplication.CreateBuilder(args);
builder
    .AddPresentation()
    .AddInfrastructure();

builder.Services.AddShared();
builder.Services.AddHttpClient();

var app = builder.Build();

// Debugging and exception page
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Request/Start pipeline
await app.TryMigrateDbAsync();

// Endpoints and routes
app.UseHttpsRedirection();

app.UseStaticFiles();
app.MapStaticAssets();
app.UseAntiforgery();

app.UseCors(c =>
{
    c.AllowAnyHeader();
    c.AllowAnyMethod();
    c.AllowAnyOrigin();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(h.Client._Imports).Assembly);

app.UseMiddleware<BadRequestResponseMiddleware>();

app.MapCarter();

app.MapHub<MatchmakingHub>(IMatchmakingHubClient.Route);
app.MapHub<MultiplayerGameSessionHub>(IMultiplayerGameSessionHubClient.Route);
app.MapHub<GameInvitationHub>(IGameInvitationHubClient.Route);

app.Run();

public partial class Program { }