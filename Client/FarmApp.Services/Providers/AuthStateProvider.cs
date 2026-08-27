using FarmApp.Services.Auth;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;

namespace FarmApp.Services.Providers;

public class AuthStateProvider
{
    private readonly IAppStoreService _appStoreService;
    private bool _isLoggedIn;
    private bool _isSessionExpired;
    public bool IsInitialized { get; private set; }
    public bool IsUserLoggedIn => _isLoggedIn;
    public bool IsSessionExpired => _isSessionExpired;

    public event Action<bool>? OnSignIn;
    public event Func<bool, Task>? OnSignInAsync;

    public AuthStateProvider(IAppStoreService appStoreService)
    {
        _appStoreService = appStoreService;
        // Sync read so routing (FlexibleRouter / PageResolveProvider) sees correct state before
        // Blazor runs InitializeAsync; mobile LocalStorageService supports GetItem.
        var token = _appStoreService.GetItem<string>(Constants.JwtDetails.ACCESS_TOKEN);
        _isLoggedIn = !string.IsNullOrEmpty(token);
    }

    public async Task InitializeAsync()
    {
        if (IsInitialized)
            return;

        var token = await _appStoreService.GetItemAsync<string>(Constants.JwtDetails.ACCESS_TOKEN);

        _isLoggedIn = !string.IsNullOrEmpty(token);

        _isSessionExpired = false;
        IsInitialized = true;

        RaiseAuthChanged();

    }
    public async Task NotifyUserAuthentication()
    {
        await NotifyUserUpdateToken();

        _isLoggedIn = true;

        RaiseAuthChanged();
    }
    public void MarkSessionExpired()
    {
        _isSessionExpired = true;

        _isLoggedIn = false;

        RaiseAuthChanged();
    } 
    public async Task NotifyUserUpdateToken()
    {
        var token = await _appStoreService.GetItemAsync<string>(Constants.JwtDetails.ACCESS_TOKEN);

        _isLoggedIn = !string.IsNullOrEmpty(token);
    }

    public void NotifyUserLogout()
    {
        _isLoggedIn = false;
        RaiseAuthChanged();
    }

    private void RaiseAuthChanged()
    {
        OnSignIn?.Invoke(_isLoggedIn);

        if (OnSignInAsync != null)
            _ = OnSignInAsync.Invoke(_isLoggedIn);
    }
}