using FarmApp.Services.Services.Interfaces;
using System.Globalization;

namespace FarmApp.Services.Services
{
    public class LocalizationService : ILocalizationService
    {
        private readonly IAppStoreService _appStoreService;
        public LocalizationService(IAppStoreService appStoreService)
        {
            _appStoreService = appStoreService;
        }
        private const string LanguageKey = "app_lang";

        private string? _currentLanguage;

        public string CurrentLanguage
        {
            get
            {
                if (_currentLanguage != null)
                    return _currentLanguage;

                var lang = _appStoreService.GetItem<string>(LanguageKey);

                if (string.IsNullOrWhiteSpace(lang))
                {
                    lang = NormalizeLanguage(
                        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);

                    _appStoreService.SetItem(LanguageKey, lang);
                }

                _currentLanguage = lang;

                return _currentLanguage;
            }
        }

        public event Action? LanguageChanged;

        public void Initialize()
        {
            string lang;

            if (_appStoreService.GetItem<string>(LanguageKey) is string storedLang)
            {
                lang = storedLang;
            }
            else
            {
                var deviceLang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

                lang = NormalizeLanguage(deviceLang);

                _appStoreService.SetItem(LanguageKey, lang);
            }

            ApplyCulture(lang);
        }

        public async Task SetLanguageAsync(string languageCode)
        {
            languageCode = NormalizeLanguage(languageCode);

            if (languageCode == CurrentLanguage)
                return;

            await _appStoreService.SetItemAsync(LanguageKey, languageCode);

            _currentLanguage = languageCode;

            ApplyCulture(languageCode);

            LanguageChanged?.Invoke();
        }
        private void ApplyCulture(string language)
        {
            var culture = new System.Globalization.CultureInfo(language);

            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
        private string NormalizeLanguage(string language)
        {
            return language switch
            {
                "uk" => "uk",
                "en" => "en",
                "pl" => "pl",
                "it" => "it",
                "de" => "de",
                _ => "en"
            };
        }
    }
}
