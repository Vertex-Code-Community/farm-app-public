using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using FarmApp.PmtilesTileGateway.Configuration;
using FarmApp.PmtilesTileGateway.Fallbacks;

namespace FarmApp.PmtilesTileGateway.Endpoints;

public static class StyleEndpoint
{
    private static readonly HashSet<string> SupportedStyles = new(StringComparer.OrdinalIgnoreCase)
    {
        "osm-bright",
        "dark-matter-gl-style"
    };

    public static IEndpointRouteBuilder MapStyleEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/styles/{styleName}/style.json", HandleStyle);
        return app;
    }

    private static async Task<IResult> HandleStyle(
        string styleName,
        IHttpClientFactory httpFactory,
        GatewayOptions options,
        IHostEnvironment env,
        ILoggerFactory loggerFactory)
    {
        if (!SupportedStyles.Contains(styleName))
            return Results.NotFound();

        var logger = loggerFactory.CreateLogger("FarmApp.PmtilesTileGateway.Style");
        var json = await TryLoadLocalStyle(env, styleName, logger)
                   ?? await TryLoadFromSpaces(httpFactory.CreateClient("styles"), options, styleName, logger);

        if (json is null)
            return BuiltInStyle(styleName, options);

        var root = JsonNode.Parse(json)?.AsObject();
        if (root is null)
            return Results.Problem("Invalid style JSON", statusCode: 500);
        if (root["sources"] is JsonObject sources)
            RewriteVectorSourcesToLocalTileJson(sources, options.TileJsonUrl);

        // glyphs URL intentionally NOT rewritten: gateway can't merge multi-font stacks
        // (Metropolis,Noto Sans) the way upstream tile.openstreetmap.org.ua does, so for
        // Cyrillic ranges it would fall back to Metropolis-solo and return empty glyphs.

        if (string.Equals(styleName, "dark-matter-gl-style", StringComparison.OrdinalIgnoreCase)
            && options.DarkMatterStyleLightenAmount > 0)
        {
            StyleJsonColorLighten.ApplyDarkTheme(
                root,
                options.DarkMatterStyleLightenAmount,
                options.DarkMatterStyleLightenMaxChannelAvg);
        }

        return Results.Text(
            root.ToJsonString(new JsonSerializerOptions { WriteIndented = false }),
            "application/json",
            Encoding.UTF8);
    }

    private static async Task<string?> TryLoadLocalStyle(IHostEnvironment env, string styleName, ILogger logger)
    {
        var path = Path.Combine(env.ContentRootPath, "wwwroot", "styles", styleName, "style.json");
        if (!File.Exists(path)) return null;

        try
        {
            var json = await File.ReadAllTextAsync(path);
            logger.LogInformation("Style loaded from local file {Path}", path);
            return json;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Local style read failed for {Path}", path);
            return null;
        }
    }

    private static async Task<string?> TryLoadFromSpaces(
        HttpClient client, GatewayOptions options, string styleName, ILogger logger)
    {
        var url = $"{options.SourceStyleBase}/styles/{styleName}/style.json";
        try
        {
            var resp = await client.GetAsync(url);
            if (resp.IsSuccessStatusCode)
            {
                logger.LogInformation("Style loaded from {Url}", url);
                return await resp.Content.ReadAsStringAsync();
            }
            logger.LogWarning("Style {Url} returned {Code}, using built-in fallback", url, (int)resp.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Style fetch failed {Url}, using built-in fallback", url);
        }
        return null;
    }

    private static IResult BuiltInStyle(string styleName, GatewayOptions options)
    {
        var isDark = string.Equals(styleName, "dark-matter-gl-style", StringComparison.OrdinalIgnoreCase);
        var fallback = FallbackJsonBuilder.Style(options.TileJsonUrl, options.GlyphsUrlTemplate, isDark);
        return Results.Text(fallback.ToJsonString(), "application/json", Encoding.UTF8);
    }

    private static void RewriteVectorSourcesToLocalTileJson(JsonObject sources, string tileJsonUrl)
    {
        foreach (var kv in sources)
        {
            if (kv.Value is not JsonObject s) continue;
            if (s["type"]?.ToString() != "vector") continue;

            if (s["url"] is JsonValue u && ShouldRewriteUrl(u.ToString()))
            {
                s["url"] = tileJsonUrl;
                s.Remove("tiles");
            }
            else if (s["tiles"] is JsonArray tArr
                     && tArr.Count > 0
                     && tArr[0] is JsonValue tv
                     && tv.ToString().StartsWith("pmtiles://", StringComparison.OrdinalIgnoreCase))
            {
                s["url"] = tileJsonUrl;
                s.Remove("tiles");
            }
        }
    }

    private static bool ShouldRewriteUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return false;
        return url.StartsWith("pmtiles://", StringComparison.OrdinalIgnoreCase)
               || url.Contains("data/v3.json", StringComparison.OrdinalIgnoreCase);
    }
}
