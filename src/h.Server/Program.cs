using Carter;
using h.Server.Components;
using h.Server.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder
    .AddPresentation()
    .AddInfrastructure();

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
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(h.Client._Imports).Assembly);


app.MapCarter();

app.Run();
