using FarmApp.Services.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Layouts.AppLayout
{
    public partial class AppLayout : IDisposable
    {
        [Inject] public required IThemeService ThemeService { get; set; }
        
        protected override void OnInitialized()
        {
            ThemeService.ThemeChanged += StateHasChanged;
        }

        public void Dispose()
        {
            ThemeService.ThemeChanged -= StateHasChanged;
        }
    }
}
