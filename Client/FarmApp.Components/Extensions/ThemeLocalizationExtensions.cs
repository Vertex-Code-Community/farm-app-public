using FarmApp.Models.Theme;
using FarmApp.Shared.Resources.Localization;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Extensions
{
    public static class ThemeLocalizationExtensions
    {
        public static string ToLocalizedThemeName(this AppThemeMode theme, IStringLocalizer<AppRecources> Localizer)
        {
            return Localizer[$"Theme_{theme}"];
        }
    }
}
