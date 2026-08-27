using System.Text;
using FarmApp.PmtilesTileGateway.Configuration;
using FarmApp.PmtilesTileGateway.Services;

namespace FarmApp.PmtilesTileGateway.Endpoints;

public static class DiagnosticEndpoints
{
    private const string HelpText =
        "PMTiles → HTTPS vector tiles. GET /readyz, /test, /data/v3.json, /tiles/{z}/{x}/{y}.pbf, /styles/…/style.json\n"
        + "404 on some tiles is normal for sparse archives (no geometry in that cell). 502 on tiles = read/decompress error (check logs).\n"
        + "If the map looks vertically flipped vs the web pmtiles viewer, try config PmtilesTileYScheme=tms (default xyz).\n"
        + "RemoteCertificateNameMismatch on PmtilesUrl: set PmtilesInsecureSkipTlsVerify=true only for dev, or use a PmtilesUrl whose hostname matches the TLS cert.";

    public static IEndpointRouteBuilder MapDiagnosticEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => Results.Text(HelpText, "text/plain", Encoding.UTF8));
        app.MapGet("/healthz", () => Results.Ok("ok"));
        app.MapGet("/readyz", Readyz);
        app.MapGet("/test", Test);
        return app;
    }

    private static IResult Readyz(IPmtilesReaderProvider provider, GatewayOptions options) =>
        Results.Json(new
        {
            pmtilesReady = provider.Reader is not null,
            pmtilesUrl = string.IsNullOrWhiteSpace(options.PmtilesLocalPath) ? options.PmtilesUrl : null,
            pmtilesLocalPath = string.IsNullOrWhiteSpace(options.PmtilesLocalPath) ? null : options.PmtilesLocalPath,
            pmtilesTileYScheme = options.YScheme,
            pmtilesInsecureSkipTlsVerify = options.PmtilesInsecureSkipTlsVerify,
            error = provider.InitError
        });

    private static IResult Test(IPmtilesReaderProvider provider, GatewayOptions options) =>
        Results.Json(new
        {
            status = "ok",
            timestamp = DateTime.UtcNow,
            pmtilesReady = provider.Reader is not null,
            pmtilesError = provider.InitError,
            config = new
            {
                publicBaseUrl = options.PublicBase,
                pmtilesUrl = options.PmtilesUrl,
                pmtilesLocalPath = options.PmtilesLocalPath ?? string.Empty,
                pmtilesTileYScheme = options.YScheme,
                sourceStyleBase = options.SourceStyleBase
            },
            endpoints = new
            {
                tileJson = options.TileJsonUrl,
                tileSample = $"{options.PublicBase}/tiles/0/0/0.pbf",
                styles = new[]
                {
                    $"{options.PublicBase}/styles/osm-bright/style.json",
                    $"{options.PublicBase}/styles/dark-matter-gl-style/style.json"
                },
                fontsExample = $"{options.PublicBase}/fonts/Open Sans Regular/0-255.pbf",
                readyz = $"{options.PublicBase}/readyz",
                healthz = $"{options.PublicBase}/healthz"
            }
        });
}
