using FarmApp.Models.Theme;

namespace FarmApp.Services.Services.Interfaces
{
    public interface IThemeStorage
    {
        AppThemeMode? Get();
        void Set(AppThemeMode mode);
    }
}
