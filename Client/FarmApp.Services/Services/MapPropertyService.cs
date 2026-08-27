using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.Properties;
using FarmApp.ViewModels.PropertyNoteStatuses;

namespace FarmApp.Services.Services;

public class MapPropertyService : IMapPropertyService, IDisposable
{
    private readonly IStateService _stateService;
    private readonly IPropertyService _propertyService;
    private readonly AuthStateProvider _authenticationStateProvider;
    private readonly IPropertyNoteStatusService _propertyNoteStatusService;
    private readonly IGlobalLoaderService _globalLoader;
    
    public MapPropertyService(IStateService stateService, 
        IPropertyService propertyService, 
        IPropertyNoteStatusService propertyNoteStatusService,
        AuthStateProvider authenticationStateProvider,
        IGlobalLoaderService globalLoaderService)
    {
        _stateService = stateService;
        _propertyService = propertyService;
        _authenticationStateProvider = authenticationStateProvider;
        _propertyNoteStatusService = propertyNoteStatusService;
        _globalLoader = globalLoaderService;

        IMapCallbackService.MapIsLoaded.Subscribe(OnMapLoadedAsync);
        _authenticationStateProvider.OnSignInAsync += OnUserSignInAsync;
    }
    
    public void Dispose()
    {
        IMapCallbackService.MapIsLoaded.Unsubscribe(OnMapLoadedAsync);
        _authenticationStateProvider.OnSignInAsync -= OnUserSignInAsync;
    }
    
    private async Task OnMapLoadedAsync()
    {
        if (!_authenticationStateProvider.IsUserLoggedIn)
        {
            _stateService.Clear();
            IMapPropertyService.InvokeDrawProperties(new List<PropertyViewModel>());
            return;
        }
        using var loader = _globalLoader.SwitchOn();

        _stateService.Clear();
        _stateService.PropertiesTask ??= GetPropertiesAsync();

        var properties = await _stateService.PropertiesTask;
        _stateService.AddPropertyNoteStatuses(await GetStatusesAsync());

        IMapPropertyService.InvokeDrawProperties(properties);
    }
    
    private async Task OnUserSignInAsync(bool signIn)
    {
        if (signIn) await OnMapLoadedAsync();
        else
        {
            _stateService.Clear();
            IMapPropertyService.InvokeDrawProperties(new List<PropertyViewModel>());
        }
    }

    private async Task<List<PropertyViewModel>> GetPropertiesAsync()
    {
        var isAuthorized = _authenticationStateProvider.IsUserLoggedIn;
        await Task.Delay(2000);

        var properties = await _propertyService.GetAllOfCurrentUserAsync(isAuthorized, isAuthorized);
        await _stateService.AddPropertiesAsync(properties);
        
        return _stateService.Properties;
    }

    private async Task<List<PropertyNoteStatusModel>> GetStatusesAsync()
    {
        var isAuthorized = _authenticationStateProvider.IsUserLoggedIn;

        var result = await _propertyNoteStatusService.GetAllAsync(isAuthorized, isAuthorized);

        return result;
    }
}