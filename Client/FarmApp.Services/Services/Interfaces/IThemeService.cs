using FarmApp.Models.Theme;

namespace FarmApp.Services.Services.Interfaces
{
    public interface IThemeService
    {
        AppThemeMode Current { get; }
        event Action? ThemeChanged;
        string GetResolvedTheme();
        void SetTheme(AppThemeMode theme);
    }
}
