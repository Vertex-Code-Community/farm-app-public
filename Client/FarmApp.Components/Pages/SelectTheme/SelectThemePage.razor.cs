using FarmApp.Models.Theme;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Resources.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmApp.Components.Pages.SelectTheme
{
    public partial class SelectThemePage
    {
        [Inject] public required IThemeService ThemeService { get; set; }
        [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
        protected override void OnInitialized()
        {
            ThemeService.ThemeChanged += StateHasChanged;
        }
        private void SetTheme(AppThemeMode theme)
        {
            ThemeService.SetTheme(theme);
        }
        public void Dispose()
        {
            ThemeService.ThemeChanged -= StateHasChanged;
        }
    }
}
