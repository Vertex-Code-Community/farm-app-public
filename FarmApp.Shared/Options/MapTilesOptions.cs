namespace FarmApp.Shared.Options;

/// <summary>
/// Optional app settings. Planet / world map via <c>pmtiles://</c> in Blazor uses
/// <c>map-config.js</c> (WASM + maplibre-gl + pmtiles.js). The native map cannot load
/// <c>pmtiles://</c> — use a gateway like <c>FarmApp.PmtilesTileGateway</c> (HTTPS TileJSON
/// + <c>/{z}/{x}/{y}.pbf</c>) and set <see cref="NativeTileGatewayBaseUrl"/> so <c>MapStyleState</c>
/// points at that style; otherwise the app falls back to tile.openstreetmap.org.ua.
/// </summary>
public class MapTilesOptions
{
    public const string SectionName = "MapTiles";

    /// <summary>Reserved (e.g. feature flags).</summary>
    public bool UseSpacesMapStyles { get; set; }

    /// <summary>Spaces / web map base; used by tooling and Blazor, not the native style URL by itself.</summary>
    public string StyleBaseUrl { get; set; } = "https://YOUR-STYLE-BASE-URL-HERE/map";

    /// <summary>
    /// Public base URL of FarmApp.PmtilesTileGateway (no trailing slash), e.g. <c>https://tiles.example.com</c> or
    /// <c>http://10.0.2.2:5088</c> to the dev machine from Android emulator. When non-empty, native map uses
    /// <c>{base}/styles/osm-bright|dark-matter-gl-style/style.json</c> (HTTP TileJSON, no <c>pmtiles://</c>).
    /// </summary>
    public string? NativeTileGatewayBaseUrl { get; set; }
}
