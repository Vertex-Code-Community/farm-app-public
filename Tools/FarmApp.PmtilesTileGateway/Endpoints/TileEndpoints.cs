using System.Text;
using FarmApp.PmtilesTileGateway.Configuration;
using FarmApp.PmtilesTileGateway.Services;

namespace FarmApp.PmtilesTileGateway.Endpoints;

public static class TileEndpoints
{
    public static IEndpointRouteBuilder MapTileEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/data/v3/{z:int}/{x:int}/{y:int}.pbf", HandleTile);
        app.MapGet("/tiles/{z:int}/{x:int}/{y:int}.pbf", HandleTile);
        return app;
    }

    private static async Task<IResult> HandleTile(
        int z, int x, int y,
        IPmtilesReaderProvider provider,
        GatewayOptions options,
        ILoggerFactory loggerFactory,
        IHostEnvironment env)
    {
        provider.EnsureInitialized();
        if (provider.Reader is null) return PmtilesUnavailable(provider.InitError, env);

        var yLookup = options.YScheme == "tms" ? (1 << z) - 1 - y : y;

        try
        {
            var stream = await provider.Reader.GetTileZxyAsync(z, x, yLookup);
            if (stream is null) return Results.NotFound();
            return Results.Stream(stream, "application/x-protobuf");
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("FarmApp.PmtilesTileGateway.Tiles")
                .LogError(ex, "PMTiles read failed for client tile {Z}/{X}/{Y} (lookup y={YLookup})",
                    z, x, y, yLookup);
            return Results.Problem("Tile read failed (see server logs).",
                statusCode: StatusCodes.Status502BadGateway);
        }
    }

    private static IResult PmtilesUnavailable(string? error, IHostEnvironment env) =>
        Results.Text(
            "PMTiles reader is not available.\n\n"
            + (error ?? "No error text. Check server logs.")
            + (env.IsDevelopment()
                ? "\n\nUse GET /readyz (JSON) and check PmtilesUrl / PmtilesLocalPath."
                : string.Empty),
            "text/plain",
            Encoding.UTF8,
            statusCode: StatusCodes.Status503ServiceUnavailable);
}
