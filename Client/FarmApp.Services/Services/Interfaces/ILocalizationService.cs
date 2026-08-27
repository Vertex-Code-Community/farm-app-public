namespace FarmApp.Services.Services.Interfaces
{
    public interface ILocalizationService
    {
        event Action? LanguageChanged;
        string CurrentLanguage { get; }
        void Initialize();
        Task SetLanguageAsync(string languageCode);
    }
}
