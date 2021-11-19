using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CebWasm;
using CompteEstBon;
using Syncfusion.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped(sp => new CebTirage());
builder.Services.AddSyncfusionBlazor();
await builder.Build().RunAsync();
