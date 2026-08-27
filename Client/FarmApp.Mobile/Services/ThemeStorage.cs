using FarmApp.Models.Theme;
using FarmApp.Services.Services.Interfaces;

namespace FarmApp.Mobile.Services
{
    public class ThemeStorage : IThemeStorage
    {
        private const string Key = "app_theme";
        public AppThemeMode? Get()
        {
            if (!Preferences.ContainsKey(Key))
                return null;

            var value = Preferences.Get(Key, nameof(AppThemeMode.System));
            return Enum.Parse<AppThemeMode>(value);
        }

        public void Set(AppThemeMode theme)
        {
            Preferences.Set(Key, theme.ToString());
        }
    }
}
