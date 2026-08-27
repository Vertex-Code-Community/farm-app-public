using FarmApp.Services.Services.Interfaces;
using Microsoft.JSInterop;

namespace FarmApp.WebClient.Services
{
    public class WebThemeDetector : IThemeDetector
    {
        private readonly IJSRuntime jSRuntime;
        public WebThemeDetector(IJSRuntime jSRuntime)
        {
            this.jSRuntime = jSRuntime;
        }

        public string GetSystemTheme()
            => jSRuntime.InvokeAsync<bool>(
                "window.matchMedia('(prefers-color-scheme: dark)').matches").Result
                 ? "dark"
                 : "light";
    }
}
