using h.Client.Services;
using h.Client.Services.Game;
using h.Contracts;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<HeadOutlet>("head::after");

// Todo: add polly?
builder.Services.AddSingleton(new HttpClient()
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
});

builder.Services.AddTransient<ToastService>();

builder.Services.AddSingleton<IWasmHttpClient, WasmHttpClient>();
builder.Services.AddSingleton<IWasmGameService, WasmGameService>();

builder.Services.AddShared();

await builder.Build().RunAsync();
