using Microsoft.AspNetCore.Components;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Enums;
using FarmApp.ViewModels.Users;
using FarmApp.ViewModels.Email;
using FarmApp.Services.Providers;
using FarmApp.Components.Services.Interfaces;
using FarmApp.Shared.Helpers;
using Microsoft.Extensions.Localization;
using FarmApp.Shared.Resources.Localization;

namespace FarmApp.Components.Pages.Profile;

public partial class ProfilePage : IDisposable
{
    [Inject] public required INavigationService NavigationService { get; set; }
    [Inject] public required IAuthenticationService AuthenticationService { get; set; }
    [Inject] public required ILocalizationService LocalizationService { get; set; }
    [Inject] public required IStateService StateService { get; set; }
    [Inject] public required IThemeService ThemeService { get; set; }
    [Inject] public required IUserService UserService { get; set; }
    [Inject] public required IMessageService MessageService { get; set; }
    [Inject] public required INotificationService NotificationService { get; set; }
    [Inject] public required IGlobalLoaderService GlobalLoaderService { get; set; }
    [Inject] public required IDialogService DialogService { get; set; }
    [Inject] public required AuthStateProvider AuthStateProvider { get; set; }
    [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
    [Inject] public required ITabsService TabsService { get; set; }
    [Inject] public required ICountryMapStorage CountryMapStorage { get; set; }

    private UserModel? _userModel;
    private bool _showLogoutModal = false;
    private string _displayName = string.Empty;
    private bool _isLoading = true;
    private bool _isContactModalOpened;
    private string _contactUsMessage = string.Empty;
    private string _selectedLanguage = "English";
    private AppMapCountry _selectedCountry = AppMapCountry.Ukraine;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    protected override async Task OnInitializedAsync()
    {
        LocalizationService.LanguageChanged += OnLanguageChanged;
        CountryMapStorage.MapCountryChanged += OnCountryChanged;

        ApplyLanguage();
        _selectedCountry = CountryMapStorage.Get() ?? AppMapCountry.Ukraine;
        var userModelTask = UserService.GetCurrentUserAsync(_cancellationTokenSource.Token);
        var minimalTimeTask = Task.Delay(TimeSpan.FromMilliseconds(750), _cancellationTokenSource.Token);

        await Task.WhenAll(userModelTask, minimalTimeTask);
        _userModel = await userModelTask;
        _displayName = BuildDisplayName(_userModel);
        _isLoading = false;

        NavigationService.LocationChanged -= OnNavigationLocationChanged;
        NavigationService.LocationChanged += OnNavigationLocationChanged;

        StateHasChanged();
    }

    private async void OnNavigationLocationChanged()
    {
        if (!string.Equals(NavigationService.CurrentPage?.Route, Constants.ClientRoutes.ProfilePage, StringComparison.Ordinal))
            return;

        try
        {
            _userModel = await UserService.GetCurrentUserAsync(_cancellationTokenSource.Token);
            _displayName = BuildDisplayName(_userModel);
            await InvokeAsync(StateHasChanged);
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
    }
    private void OnLanguageChanged()
    {
        ApplyLanguage();

        StateHasChanged();
    }

    private void OnCountryChanged(AppMapCountry country)
    {
        _selectedCountry = country;
        InvokeAsync(StateHasChanged);
    }

    private static string FlagFile(AppMapCountry country) => country switch
    {
        AppMapCountry.Ukraine => "UA.svg",
        AppMapCountry.UnitedStates => "US.svg",
        AppMapCountry.Italy => "IT.svg",
        AppMapCountry.Poland => "PL.svg",
        AppMapCountry.Germany => "DE.svg",
        _ => "UA.svg"
    };

    private string LocalizedCountryName(AppMapCountry country) => country switch
    {
        AppMapCountry.Ukraine => Localizer["Country_Ukraine"],
        AppMapCountry.UnitedStates => Localizer["Country_US"],
        AppMapCountry.Italy => Localizer["Country_Italy"],
        AppMapCountry.Poland => Localizer["Country_Poland"],
        AppMapCountry.Germany => Localizer["Country_Germany"],
        _ => country.ToString()
    };
    private void ApplyLanguage()
    {
        _selectedLanguage = LanguageHelper.NormalizeLanguage(LocalizationService.CurrentLanguage);
    }
    private void OnLogout()
    {

        AuthenticationService.LogOut();
        NavigationService.History.Clear();
        StateService.Clear();
        AuthStateProvider.NotifyUserLogout();
        
        NavigationService.NavigateTo(Constants.ClientRoutes.WelcomePage);
    }
    private void ChangeTabVisibility(bool Show)
    {
        TabsService.SwitchVisibility(Show);
    }

    private void ContactUsClicked()
    {
        _isContactModalOpened = true;
    }

    private async Task SendMessage()
    {
        using var loader = GlobalLoaderService.SwitchOn();
        
        var result = await MessageService.ContactUsAsync(new ContactUsModel
        {
            FromApp = true, 
            Message = _contactUsMessage
        }, _cancellationTokenSource.Token);
        
        _contactUsMessage = string.Empty;
        _isContactModalOpened = false;

        var message = result?.IsSuccess ?? false ? "Повідомлення відправлено. <br>Дякуємо за звернення!" : "Повідомлення не відправлено. <br>Сталася помилка. <br>Спробуйте будь ласка ще раз!";
        NotificationService.Add("Результат", message);
    }

    private void OnGuideClicked()
    {
        NavigationService.NavigateTo(Constants.ClientRoutes.GuidePage, new Dictionary<string, object>
        {
            { "ShowBackButton", true }
        });
    }

    public void Dispose()
    {
        NavigationService.LocationChanged -= OnNavigationLocationChanged;
        CountryMapStorage.MapCountryChanged -= OnCountryChanged;
        _cancellationTokenSource.Cancel();
        LocalizationService.LanguageChanged -= OnLanguageChanged;
    }

    private static string BuildDisplayName(UserModel? user)
    {
        if (user is null) return string.Empty;
        var fullName = string.Join(' ', new[] { user.FirstName, user.LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
        if (!string.IsNullOrWhiteSpace(fullName))
            return fullName.Trim();
        if (!string.IsNullOrWhiteSpace(user.UserName))
            return user.UserName;
        return user.Email;
    }
}
