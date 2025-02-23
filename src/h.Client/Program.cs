using Blazored.SessionStorage;
using h.Client.Services;
using h.Client.Services.Game;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Refit;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTransient<ToastService>();
builder.Services.AddBlazoredSessionStorage();

// Todo: Deprecate this http client & switch to refit
builder.Services.AddSingleton(new HttpClient()
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
});
builder.Services.AddSingleton<IWasmHttpClient, WasmHttpClient>();

builder.Services.AddScoped<TokenRefreshingDelegatingHandler>();
builder.Services
    .AddRefitClient<IHApiClient>(new RefitSettings()
    {
        ContentSerializer = new SystemTextJsonContentSerializer(AppJsonOptions.WithConverters),
    })
    .ConfigureHttpClient(c =>
    {
        // Add polly?
        c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    })
    .AddHttpMessageHandler<TokenRefreshingDelegatingHandler>();

builder.Services
    .AddRefitClient<IHTokenRefreshClient>(new RefitSettings()
    {
        ContentSerializer = new SystemTextJsonContentSerializer(AppJsonOptions.WithConverters),
    })
    .ConfigureHttpClient(c =>
    {
        // Add polly?
        c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    });


builder.Services.AddSingleton<IWasmGameService, WasmGameService>();

builder.Services.AddScoped<IWasmCurrentUserStateService,WasmCurrentUserStateService>();

builder.Services.AddScoped<WasmAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, WasmAuthenticationStateProvider>(f =>
    f.GetRequiredService<WasmAuthenticationStateProvider>());

builder.Services.AddAuthorizationCore(o =>
{
    o.AddAppClientPolicies();
});
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

await builder.Build().RunAsync();
