using h.Client.Services;
using h.Contracts.Components.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Todo: add polly?
builder.Services.AddSingleton(new HttpClient()
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
});
builder.Services.AddSingleton<IWasmOnlyHttpClient, WasmOnlyHttpClient>();

await builder.Build().RunAsync();
