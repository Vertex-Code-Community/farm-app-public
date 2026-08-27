using Bch.Components.Modal.Root;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FarmApp.Admin.Wasm;
using FarmApp.AdminComponents.Extensions;
using FarmApp.Shared.Constants;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.RootComponents.Add<BchRootModal>("#bch-modal");

var services = builder.Services;
var configuration = builder.Configuration;

var apiBaseUrl = configuration ["ApiBaseUrl"];

services.AddScoped<CustomAuthorizationMessageHandler>();

services.AddHttpClient(ApiType.ManagementAppApi, client =>
    {
        client.BaseAddress = string.IsNullOrEmpty(apiBaseUrl)
            ? new Uri(builder.HostEnvironment.BaseAddress)
            : new Uri(apiBaseUrl);
        client.Timeout = TimeSpan.FromMinutes(10);
    })
    .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

services.AddAdminAppDependencies();
await builder.Build().RunAsync();