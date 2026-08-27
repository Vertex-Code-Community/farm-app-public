using FarmApp.Services.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using FarmApp.Shared.Helpers;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using FarmApp.Shared.Resources.Localization;

namespace FarmApp.Components.Pages.SelectLanguage
{
    public partial class SelectLanguagePage : IDisposable
    {
        [Inject] public required ILocalizationService LocalizationService { get; set; }
        [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
        private List<string> Languages = new() { "English", "Українська", "Italiano", "Polski", "Deutsch" };

        private string _selectedLanguage = "English";
        private bool _isLoading = false;
        protected override void OnInitialized()
        {
            string lang = LanguageHelper.NormalizeLanguage(LocalizationService.CurrentLanguage);
            _selectedLanguage = lang;
            LocalizationService.LanguageChanged += StateHasChanged;
        }
        private async Task SelectLanguage(string language)
        {
            _isLoading = true;
            string languageCode = LanguageHelper.GetLanguageCode(language);
            _selectedLanguage = language;

            await Task.WhenAll(LocalizationService.SetLanguageAsync(languageCode), Task.Delay(600));
            _isLoading = false;
        }

        public void Dispose()
        {
            LocalizationService.LanguageChanged -= StateHasChanged;
        }
    }
}
