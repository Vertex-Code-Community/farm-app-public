using System.Text.Json.Nodes;

namespace FarmApp.PmtilesTileGateway.Fallbacks;

public static class FallbackJsonBuilder
{
    public static JsonObject TileJson() => new()
    {
        ["tilejson"] = "3.0.0",
        ["name"] = "OpenMapTiles",
        ["id"] = "openmaptiles",
        ["format"] = "pbf",
        ["scheme"] = "xyz",
        ["minzoom"] = 0,
        ["maxzoom"] = 14,
        ["bounds"] = new JsonArray { -180.0, -85.0511, 180.0, 85.0511 },
        ["center"] = new JsonArray { 0.0, 0.0, 2 },
        ["attribution"] =
            "<a href=\"https://openmaptiles.org/\">© OpenMapTiles</a> "
            + "<a href=\"https://www.openstreetmap.org/copyright\">© OpenStreetMap contributors</a>"
    };

    public static JsonObject Style(string tileJsonUrl, string glyphsUrl, bool isDark)
    {
        var palette = isDark ? DarkPalette : BrightPalette;

        return new JsonObject
        {
            ["version"] = 8,
            ["name"] = isDark ? "FarmApp Dark" : "FarmApp Bright",
            ["sources"] = new JsonObject
            {
                ["openmaptiles"] = new JsonObject
                {
                    ["type"] = "vector",
                    ["url"] = tileJsonUrl
                }
            },
            ["glyphs"] = glyphsUrl,
            ["layers"] = new JsonArray
            {
                FillLayer("background", "background", null, palette.Background),
                FillLayer("landuse", null, "landuse", palette.Land),
                FillLayer("water", null, "water", palette.Water),
                LineLayer("boundary", "boundary", palette.Boundary, 1.0),
                LineLayer("transportation", "transportation", palette.Road, 1.2),
                PlaceLabelLayer(palette.Label)
            }
        };
    }

    private record Palette(string Background, string Water, string Land, string Road, string Boundary, string Label);

    private static readonly Palette BrightPalette = new(
        Background: "#f8f4f0",
        Water: "#a0c8f0",
        Land: "#e6e6e6",
        Road: "#ffffff",
        Boundary: "#888888",
        Label: "#444444");

    private static readonly Palette DarkPalette = new(
        Background: "#1a1a1a",
        Water: "#0e1626",
        Land: "#222226",
        Road: "#3a3a3a",
        Boundary: "#555555",
        Label: "#cccccc");

    private static JsonObject FillLayer(string id, string? type, string? sourceLayer, string color)
    {
        var layer = new JsonObject
        {
            ["id"] = id,
            ["type"] = type ?? "fill",
            ["paint"] = new JsonObject { [type == "background" ? "background-color" : "fill-color"] = color }
        };
        if (sourceLayer is not null)
        {
            layer["source"] = "openmaptiles";
            layer["source-layer"] = sourceLayer;
        }
        return layer;
    }

    private static JsonObject LineLayer(string id, string sourceLayer, string color, double width) => new()
    {
        ["id"] = id,
        ["type"] = "line",
        ["source"] = "openmaptiles",
        ["source-layer"] = sourceLayer,
        ["paint"] = new JsonObject { ["line-color"] = color, ["line-width"] = width }
    };

    private static JsonObject PlaceLabelLayer(string color) => new()
    {
        ["id"] = "place-label",
        ["type"] = "symbol",
        ["source"] = "openmaptiles",
        ["source-layer"] = "place",
        ["layout"] = new JsonObject
        {
            ["text-field"] = "{name:latin}",
            ["text-size"] = 12,
            ["text-font"] = new JsonArray { "Open Sans Regular" }
        },
        ["paint"] = new JsonObject { ["text-color"] = color }
    };
}
