using Blazored.LocalStorage;
using BlazorClient.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.Services.AddScoped(sp =>
{
    var httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7061/") };
    return httpClient;
});
builder.Services.AddScoped<AuthService>();
builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();
