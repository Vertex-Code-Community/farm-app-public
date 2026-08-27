using FarmApp.Models.Theme;
using FarmApp.Services.Services.Interfaces;

namespace FarmApp.Services.Services
{
    public sealed class ThemeService : IThemeService
    {
        private readonly IThemeDetector _themeDetector;
        private readonly IThemeStorage _storage;

        public AppThemeMode Current { get; private set; }

        public event Action? ThemeChanged;

        public ThemeService(
            IThemeDetector themeDetector,
            IThemeStorage storage)
        {
            _themeDetector = themeDetector;
            _storage = storage;

            Current = _storage.Get() ?? AppThemeMode.System;
        }

        public string GetResolvedTheme()
        {
            return Current switch
            {
                AppThemeMode.Dark => "dark",
                AppThemeMode.Light => "light",
                _ => _themeDetector.GetSystemTheme()
            };
        }

        public void SetTheme(AppThemeMode theme)
        {
            if (Current == theme) return;

            Current = theme;
            _storage.Set(theme);
            ThemeChanged?.Invoke();
        }
    }
}
