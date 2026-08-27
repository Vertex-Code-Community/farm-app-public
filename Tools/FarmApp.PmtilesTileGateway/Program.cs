using System.Net;
using FarmApp.PmtilesTileGateway.Configuration;
using FarmApp.PmtilesTileGateway.Endpoints;
using FarmApp.PmtilesTileGateway.Services;

var builder = WebApplication.CreateBuilder(args);

var options = builder.Configuration.Get<GatewayOptions>() ?? new GatewayOptions();
options.EnsureValid();

builder.Services.AddSingleton(options);
builder.Services.AddSingleton<IPmtilesReaderProvider, PmtilesReaderProvider>();

builder.Services.AddHttpClient("styles", c =>
    {
        c.DefaultRequestHeaders.UserAgent.ParseAdd("FarmApp.PmtilesTileGateway/1.0");
        c.Timeout = TimeSpan.FromMinutes(5);
    })
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(2),
        AutomaticDecompression = DecompressionMethods.All
    });

builder.Services.AddCors(b => b.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();
app.UseCors();

app.Services.GetRequiredService<IPmtilesReaderProvider>().EnsureInitialized();

app.MapTileJsonEndpoint();
app.MapTileEndpoints();
app.MapStyleEndpoint();
app.MapFontEndpoint();
app.MapDiagnosticEndpoints();

app.Run();
