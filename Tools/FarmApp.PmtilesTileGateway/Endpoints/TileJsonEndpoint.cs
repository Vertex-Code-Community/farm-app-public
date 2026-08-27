using System.Text;
using System.Text.Json.Nodes;
using FarmApp.PmtilesTileGateway.Configuration;
using FarmApp.PmtilesTileGateway.Fallbacks;

namespace FarmApp.PmtilesTileGateway.Endpoints;

public static class TileJsonEndpoint
{
    public static IEndpointRouteBuilder MapTileJsonEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/data/v3.json", HandleTileJson);
        return app;
    }

    private static IResult HandleTileJson(GatewayOptions options)
    {
        var node = FallbackJsonBuilder.TileJson();
        node["tiles"] = new JsonArray { options.TilesUrlTemplate };
        node["id"] ??= "openmaptiles";
        return Results.Text(node.ToJsonString(), "application/json", Encoding.UTF8);
    }
}
