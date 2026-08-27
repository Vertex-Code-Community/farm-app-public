namespace FarmApp.PmtilesTileGateway.Configuration;

public sealed class GatewayOptions
{
    public string? PmtilesLocalPath { get; init; }

    public string PmtilesUrl { get; init; } =
        "https://YOUR-PMTILES-HOST-HERE/planet.pmtiles";

    public string PublicBaseUrl { get; init; } = "http://localhost:5088";

    public string SourceStyleBaseUrl { get; init; } =
        "https://YOUR-SOURCE-STYLE-HOST-HERE/map-helpers";

    public string PmtilesTileYScheme { get; init; } = "xyz";

    public bool PmtilesInsecureSkipTlsVerify { get; init; }

    public double DarkMatterStyleLightenAmount { get; init; } = 0.28;

    public double DarkMatterStyleLightenMaxChannelAvg { get; init; } = 110.0;

    public string PublicBase => PublicBaseUrl.TrimEnd('/');
    public string SourceStyleBase => SourceStyleBaseUrl.TrimEnd('/');
    public string TileJsonUrl => $"{PublicBase}/data/v3.json";
    public string TilesUrlTemplate => $"{PublicBase}/tiles/{{z}}/{{x}}/{{y}}.pbf";
    public string GlyphsUrlTemplate => $"{PublicBase}/fonts/{{fontstack}}/{{range}}.pbf";

    public string YScheme => PmtilesTileYScheme.Trim().ToLowerInvariant() is "tms" ? "tms" : "xyz";

    public void EnsureValid()
    {
        if (!Uri.TryCreate(PublicBaseUrl, UriKind.Absolute, out var u)
            || (u.Scheme != "http" && u.Scheme != "https"))
        {
            throw new InvalidOperationException(
                "PublicBaseUrl must be http(s), no trailing slash (e.g. https://tiles.example.com).");
        }
    }
}
