using FarmApp.Components.Attributes;
using FarmApp.Components.Helpers;
using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Enums;
using FarmApp.ViewModels.CustomSteads;
using FarmApp.ViewModels.Properties;
using FarmApp.ViewModels.Steads;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace FarmApp.Components.Pages.Main;

[Route(Constants.ClientRoutes.MainPage)]
[StatusBarColor(Constants.StatusbarColors.WHITE)]
public partial class MainPage : ComponentBase, IDisposable
{
    [Inject] public required IJSRuntime JsRuntime { get; set; }
    [Inject] public required IHttpService HttpService { get; set; }
    [Inject] public required AuthStateProvider AuthenticationStateProvider { get; set; }
    [Inject] public required IMapModalService MapModalService { get; set; }
    [Inject] public required IPropertyService PropertyService { get; set; }
    [Inject] public required IStateService StateService { get; set; }
    [Inject] public required ICustomSteadService CustomSteadService { get; set; }
    [Inject] public required IAdService AdService { get; set; }

    [Inject] public required INavigationService NavigationService { get; set; }

    [Parameter] public bool ShowCreateFieldHint { get; set; } = false;

    private SteadModel? _steadModel;
    private PropertyViewModel? _propertyModel;
    private CustomSteadModel? _customStead;
    private double _selectedArea = 0;
    
    private string? _selectedCustomSteadId;
    private string? _selectedPropertyId;
    private string? _selectedSteadId;

    // protected override async Task OnAfterRenderAsync(bool firstRender)
    // {
    //     if (firstRender)
    //     {
    //         await Task.Delay(500);
    //
    //         await JsRuntime.InvokeVoidAsync("keyboardLock.disableScroll");
    //     }
    // }

    protected override void OnInitialized()
    {
        IMapSteadService.OnSteadClicked += OnSteadClickedAsync;
        IMapSteadService.OnCustomSteadClicked += OnCustomSteadClickedAsync;
        IMapPropertyService.OnPropertyClicked += OnPropertyClickedAsync;

        IMapSteadService.OnUpdate += StateHasChanged;
    }

    public async void Dispose()
    {
        IMapSteadService.OnSteadClicked -= OnSteadClickedAsync;
        IMapSteadService.OnCustomSteadClicked -= OnCustomSteadClickedAsync;
        IMapPropertyService.OnPropertyClicked -= OnPropertyClickedAsync;
        
        IMapSteadService.OnUpdate -= StateHasChanged;
        
        IMapSteadService.InvokeSwitchDrawingMode(false);

        try
        {
            await JsRuntime.InvokeVoidAsync("keyboardLock.enableScroll");
        }
        catch { /* Catching JS errors */ }
    }

    private async Task OnSteadClickedAsync(string steadId, string? propertyId, float x, float y)
    {
        _selectedCustomSteadId = null;

        _selectedSteadId = steadId;
        _selectedPropertyId = propertyId;

        if (AuthenticationStateProvider.IsUserLoggedIn)
        {
            var modalId = MapModalService.Show(MapModalType.Loading, x, y);
            await LoadSteadAsync(modalId);
        }
        else MapModalService.Show(MapModalType.AdPrompt, x, y);
    }

    private async Task OnCustomSteadClickedAsync(string customSteadId, string? steadId, string? propertyId, float x, float y)
    {
        _steadModel = null;
        _selectedCustomSteadId = customSteadId;
        _selectedSteadId = steadId;
        _selectedPropertyId = propertyId;

        if (AuthenticationStateProvider.IsUserLoggedIn)
        {
            var modalId = MapModalService.Show(MapModalType.Loading, x, y);
            await LoadCustomSteadAsync(modalId);
        }
        else 
            MapModalService.Show(MapModalType.AdPrompt, x, y);
    }
    
    private async Task OnPropertyClickedAsync(string propertyId, string? steadId, string? customSteadId, float x, float y)
    {
        _steadModel = null;
        _selectedPropertyId = propertyId;
        _selectedSteadId = steadId;
        _selectedCustomSteadId = customSteadId;

        if (AuthenticationStateProvider.IsUserLoggedIn)
        {
            var modalId = MapModalService.Show(MapModalType.Loading, x, y);
            await LoadPropertyAsync(modalId);
        }
        else 
            MapModalService.Show(MapModalType.AdPrompt, x, y);
    }
    
    private async Task OnShowAdAsync()
    {
        var modalId = MapModalService.Show(MapModalType.Loading);
        
        var rewardIsLoaded = await AdService.LoadRewardAdAsync();
        if (MapModalService.ModalId != modalId) return;
        
        Console.WriteLine($"ADV rewardIsLoaded = {rewardIsLoaded}");
        
        if (!rewardIsLoaded)
        {
            MapModalService.Show(MapModalType.AdPromptFail, false);
            return;
        }
        
        var adIsShown = await AdService.ShowRewardAdAsync();
        if (MapModalService.ModalId != modalId) return;
        
        Console.WriteLine($"ADV adIsShown = {adIsShown}");
        
        if (!adIsShown)
        {
            MapModalService.Show(MapModalType.AdPromptFail, false);
            return;
        }

        var rewardEarned = await AdService.IsRewardEarnedAsync();
        if (MapModalService.ModalId != modalId) return;
        
        Console.WriteLine($"ADV rewardEarned = {rewardEarned}");
        
        if (!rewardEarned)
        {
            MapModalService.Show(MapModalType.AdPromptFail, false);
            return;
        }

        var steadClicked = !string.IsNullOrEmpty(_selectedSteadId);
        var customSteadClicked = !string.IsNullOrEmpty(_selectedCustomSteadId);
        var propertyClicked = !string.IsNullOrEmpty(_selectedPropertyId);
        
        if (steadClicked && !propertyClicked && !customSteadClicked) await LoadSteadAsync();
        else if (propertyClicked) await LoadPropertyAsync();
        else if (customSteadClicked) await LoadCustomSteadAsync();
    }

    private async Task LoadSteadAsync(string? modalId = null)
    {
        if (string.IsNullOrEmpty(modalId)) modalId = MapModalService.Show(MapModalType.Loading);
        
        var getSteadTask = HttpService.GetAsync<SteadModel>($"api/stead/{_selectedSteadId}");
        var minimalTimeTask = Task.Delay(TimeSpan.FromMilliseconds(750));
        
        await Task.WhenAll(getSteadTask, minimalTimeTask);
        if (MapModalService.ModalId != modalId) return;
        
        var steadModel = getSteadTask.Result;
        
        if (steadModel is null)
        {
            MapModalService.Hide();
            return;
        }
        
        _steadModel = steadModel;
        _selectedSteadId = null;
        
        MapModalService.Show(MapModalType.Stead, false);
        
        StateHasChanged();
    }
    
    private async Task LoadPropertyAsync(string? modalId = null)
    {
        if (string.IsNullOrEmpty(_selectedPropertyId)) return;
        if (string.IsNullOrEmpty(modalId)) modalId = MapModalService.Show(MapModalType.Loading);
        
        var propertyPreviewModelTask = PropertyService.GetPreviewByIdAsync(_selectedPropertyId);
        var minimalTimeTask = Task.Delay(TimeSpan.FromMilliseconds(750));
        
        await Task.WhenAll(propertyPreviewModelTask, minimalTimeTask);
        if (MapModalService.ModalId != modalId) return;
        
        var propertyPreviewModel = propertyPreviewModelTask.Result;
        var propertyModel = StateService.Properties.FirstOrDefault(x => x.Id == _selectedPropertyId);
        
        if (propertyPreviewModel is null || propertyModel is null)
        {
            MapModalService.Hide();
            return;
        }
        
        StateService.AddPropertyNotes(propertyPreviewModel);
        
        _propertyModel = propertyModel;
        _selectedPropertyId = null;
        
        MapModalService.Show(MapModalType.Property, false);
        
        StateHasChanged();
    }
    
    private async Task LoadCustomSteadAsync(string? modalId = null)
    {
        if (string.IsNullOrEmpty(_selectedCustomSteadId)) return;
        if (string.IsNullOrEmpty(modalId)) modalId = MapModalService.Show(MapModalType.Loading);
        
        var customSteadTask = CustomSteadService.GetByIdAsync(_selectedCustomSteadId);
        var minimalTimeTask = Task.Delay(TimeSpan.FromMilliseconds(750));
        
        await Task.WhenAll(customSteadTask, minimalTimeTask);
        if (MapModalService.ModalId != modalId) return;
        
        var customSteadModel = customSteadTask.Result;
        
        if (customSteadModel is null)
        {
            MapModalService.Hide();
            return;
        }
        
        _selectedArea = await Task.Run(() =>
        {
            try
            {
                var coords = JsonConvert.DeserializeObject<double[][]?>(customSteadModel.Coordinates);
                if (coords is null) return 0;
                
                var customSteadCoordinates = NetTopologySuiteUtils.ConvertToCoordinates(coords);
                var area = NetTopologySuiteUtils.GetAreaOfPolygon(customSteadCoordinates) / 10000;
                
                return area;
            }
            catch
            {
                return 0;
            }
        });
        
        _customStead = customSteadModel;
        _selectedCustomSteadId = null;
        
        MapModalService.Show(MapModalType.CustomStead, false);
        
        StateHasChanged();
    }
}