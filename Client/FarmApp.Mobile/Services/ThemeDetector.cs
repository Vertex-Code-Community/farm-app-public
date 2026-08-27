using FarmApp.Services.Services.Interfaces;

namespace FarmApp.Mobile.Services
{
    public class ThemeDetector : IThemeDetector
    {
        public string GetSystemTheme()
            => Application.Current?.RequestedTheme == AppTheme.Dark
                ? "dark"
                : "light";
    }
}
