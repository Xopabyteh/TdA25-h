using Blazored.SessionStorage;
using h.Client.Services;
using h.Client.Services.Game;
using h.Contracts;
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

builder.Services
    .AddRefitClient<IHApiClient>(new RefitSettings()
    {
        ContentSerializer = new SystemTextJsonContentSerializer(AppJsonOptions.WithConverters),
        //AuthorizationHeaderValueGetter = (msg, cancellationToken) => Task.FromResult("Bearer " + builder.Services.GetRequiredService<AuthenticationState>().User.GetToken())
    })
    .ConfigureHttpClient(c =>
    {
        // Add polly?
        c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    });


builder.Services.AddSingleton<IWasmGameService, WasmGameService>();

builder.Services.AddScoped<AuthenticationStateProvider, WasmAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

await builder.Build().RunAsync();
