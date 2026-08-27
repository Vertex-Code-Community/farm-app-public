using FarmApp.Models.Theme;
using FarmApp.Services.Services.Interfaces;
using Microsoft.JSInterop;

namespace FarmApp.WebClient.Services
{
    public class WebThemeStorage : IThemeStorage
    {
        private readonly IJSRuntime _js;
        public WebThemeStorage(IJSRuntime jSRuntime)
        {
            _js = jSRuntime;
        }

        public AppThemeMode? Get()
            => _js.InvokeAsync<AppThemeMode?>("themeStorage.get").Result;

        public void Set(AppThemeMode theme)
            => _js.InvokeVoidAsync("themeStorage.set", theme.ToString());
    }
}
