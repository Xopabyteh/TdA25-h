using h.Client.Services;
using h.Contracts;
using h.Contracts.Components.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Text.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Todo: add polly?
builder.Services.AddSingleton(new HttpClient()
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
});

builder.Services.AddSingleton<IWasmOnlyHttpClient, WasmOnlyHttpClient>();

builder.Services.AddShared();

await builder.Build().RunAsync();
