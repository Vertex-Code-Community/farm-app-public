using FarmApp.Models.Theme;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Options;
using Microsoft.Extensions.Options;

namespace FarmApp.Mobile.Bridges
{
    public sealed class MapStyleState
    {
        private const string FallbackBaseUrl = "https://tile.openstreetmap.org.ua";

        private readonly IThemeService _themeService;
        private readonly MapTilesOptions _mapTiles;

        public string StyleUrl { get; private set; }

        public event Action? StyleChanged;

        public MapStyleState(IThemeService themeService, IOptions<MapTilesOptions> mapTiles)
        {
            _themeService = themeService;
            _mapTiles = mapTiles.Value;
            _themeService.ThemeChanged += OnThemeChanged;
        }
        public void Initialize()
        {
            UpdateStyle();
        }
        private void OnThemeChanged()
        {
            Console.WriteLine("[MapStyleState] ThemeChanged received");
            UpdateStyle();
            StyleChanged?.Invoke();
        }
        private void UpdateStyle()
        {
          var styleName = _themeService.GetResolvedTheme() switch
            {
                "dark" => "dark-matter-gl-style",
                _ => "osm-bright"
            };

            var baseUrl = string.IsNullOrWhiteSpace(_mapTiles.NativeTileGatewayBaseUrl)
                ? FallbackBaseUrl
                : _mapTiles.NativeTileGatewayBaseUrl.TrimEnd('/');

            StyleUrl = $"{baseUrl}/styles/{styleName}/style.json";
        }
    }
}
