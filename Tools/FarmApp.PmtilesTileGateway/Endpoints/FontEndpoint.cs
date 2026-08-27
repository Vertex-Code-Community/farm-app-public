using FarmApp.PmtilesTileGateway.Configuration;

namespace FarmApp.PmtilesTileGateway.Endpoints;

public static class FontEndpoint
{
    public static IEndpointRouteBuilder MapFontEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/fonts/{*fontPath}", HandleFont);
        return app;
    }

    private static async Task<IResult> HandleFont(
        string fontPath,
        IHttpClientFactory httpFactory,
        GatewayOptions options,
        IHostEnvironment env,
        ILoggerFactory loggerFactory)
    {
        if (string.IsNullOrEmpty(fontPath)) return Results.NotFound();

        var logger = loggerFactory.CreateLogger("FarmApp.PmtilesTileGateway.Fonts");
        var paths = ExpandFontStack(fontPath);

        foreach (var candidate in paths)
        {
            var bytes = await TryReadLocal(env, candidate)
                        ?? await TryFetchUpstream(httpFactory, options, candidate, logger);
            if (bytes is not null)
                return Results.Bytes(bytes, "application/x-protobuf");
        }

        return Results.NotFound();
    }

    private static IEnumerable<string> ExpandFontStack(string fontPath)
    {
        yield return fontPath;

        var slash = fontPath.LastIndexOf('/');
        if (slash <= 0) yield break;

        var stack = fontPath[..slash];
        var range = fontPath[(slash + 1)..];
        if (!stack.Contains(',')) yield break;

        foreach (var single in stack.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            yield return $"{single.Trim()}/{range}";
        }
    }

    private static async Task<byte[]?> TryReadLocal(IHostEnvironment env, string fontPath)
    {
        var path = Path.Combine(env.ContentRootPath, "wwwroot", "fonts", fontPath);
        if (!File.Exists(path)) return null;

        try { return await File.ReadAllBytesAsync(path); }
        catch { return null; }
    }

    private static async Task<byte[]?> TryFetchUpstream(
        IHttpClientFactory httpFactory, GatewayOptions options, string fontPath, ILogger logger)
    {
        var client = httpFactory.CreateClient("styles");
        var url = $"{options.SourceStyleBase}/fonts/{fontPath}";
        try
        {
            using var resp = await client.GetAsync(url);
            if (resp.IsSuccessStatusCode)
                return await resp.Content.ReadAsByteArrayAsync();
            logger.LogDebug("Font {Url} returned {Code}", url, (int)resp.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Font fetch failed {Url}", url);
        }
        return null;
    }
}
