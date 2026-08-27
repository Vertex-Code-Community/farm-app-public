using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Bch.Modules.GlobalEvents.Services;
using FarmApp.Components.ViewModels.Tabs;
using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using Microsoft.Extensions.Localization;
using FarmApp.Shared.Resources.Localization;

namespace FarmApp.Components.Layouts.TabLayout.TabPanel;

public partial class TabPanelComponent : IAsyncDisposable
{
    [Inject] public required IAuthenticationService AuthenticationService { get; set; }
    [Inject] public required AuthStateProvider AuthStateProvider { get; set; }
    [Inject] public required INavigationService NavigationService { get; set; }
    [Inject] public required ITabsService TabsService { get; set; }
    [Inject] public required IGlobalEventsService GlobalEventsService { get; set; }
    [Inject] public required IMapSteadService MapSteadService { get; set; }
    [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
    [Inject] public required ILocalizationService LocalizationService { get; set; }

    private readonly string _key = $"_key_{Guid.NewGuid()}";

    private TabConfigModel _profileTab = new();
    private TabConfigModel _signInTab = new();

    private readonly List<TabConfigModel> _tabModels = new();

    private bool _mapMoving = false;

    private bool SuppressTabsForFieldModal => IMapPropertyService.IsFieldCreateModalOpen;

    protected override Task OnInitializedAsync()
    {
        RebuildTabs();

        AuthStateProvider.OnSignIn += CheckAndChangeTabs;
        NavigationService.LocationChanged += StateHasChanged;
        TabsService.OnVisibilityChanged += StateHasChanged;
        IMapPropertyService.OnUpdate += OnMapPropertyServiceUpdate;
        LocalizationService.LanguageChanged += OnLanguageChanged;
        // MapSteadService.OnFirstMove += OnMapFirstMove;
        SystemEventsProvider.OnLastTouchLifted += OnGlobalUp;

        return GlobalEventsService.AddDocumentListenerAsync<MouseEventArgs>("mouseup", _key, OnMouseUpAsync);
    }
    
    public async ValueTask DisposeAsync()
    {
        AuthStateProvider.OnSignIn -= CheckAndChangeTabs;
        NavigationService.LocationChanged -= StateHasChanged;
        TabsService.OnVisibilityChanged -= StateHasChanged;
        IMapPropertyService.OnUpdate -= OnMapPropertyServiceUpdate;
        LocalizationService.LanguageChanged -= OnLanguageChanged;
        // MapSteadService.OnFirstMove -= OnMapFirstMove;
        SystemEventsProvider.OnLastTouchLifted -= OnGlobalUp;
        
        await GlobalEventsService.RemoveDocumentListenerAsync<MouseEventArgs>("mouseup", _key);
    }

    private void OnLanguageChanged()
    {
        InvokeAsync(() =>
        {
            RebuildTabs();
            StateHasChanged();
        });
    }

    private void RebuildTabs()
    {
        var isAuthorized = AuthStateProvider.IsUserLoggedIn;

        _profileTab = new TabConfigModel
        {
            Name = Localizer["TabPanel_Account"],
            ImgUrl = "_content/FarmApp.Components/img/tabs/account.svg",
            Reference = Constants.ClientRoutes.ProfilePage
        };

        _signInTab = new TabConfigModel
        {
            Name = Localizer["TabPanel_Account"],
            ImgUrl = "_content/FarmApp.Components/img/tabs/account.svg",
            Reference = Constants.ClientRoutes.SignInPage
        };

        _tabModels.Clear();
        _tabModels.Add(new TabConfigModel { Name = Localizer["TabPanel_Home"], ImgUrl = "_content/FarmApp.Components/img/tabs/home.svg", Reference = Constants.ClientRoutes.HomePage });
        _tabModels.Add(new TabConfigModel { Name = Localizer["TabPanel_Map"], ImgUrl = "_content/FarmApp.Components/img/tabs/map.svg", Reference = Constants.ClientRoutes.MainPage });
        _tabModels.Add(new TabConfigModel { Name = Localizer["TabPanel_Fields"], ClassName="fields-tab guide-target", ImgUrl = "_content/FarmApp.Components/img/shared/leaf.svg", Reference = Constants.ClientRoutes.PropertiesPage, NestedReferences = new[] { Constants.ClientRoutes.PropertiesDetailsPage } });
        _tabModels.Add(isAuthorized ? _profileTab : _signInTab);
    }

    private void CheckAndChangeTabs(bool _ = false)
    {
        var isAuthorized = AuthStateProvider.IsUserLoggedIn;
        _tabModels.Remove(_profileTab);
        _tabModels.Remove(_signInTab);

        // Console.WriteLine($"CheckAndChangeTabs isAuthorized = {isAuthorized}");

        _tabModels.Add(isAuthorized ? _profileTab : _signInTab);
        StateHasChanged();
    }

    private void OnMapFirstMove()
    {
        _mapMoving = true;
        StateHasChanged();
    }

    private Task OnMouseUpAsync(MouseEventArgs _)
    {
        OnGlobalUp();
        return Task.CompletedTask;
    } 
    
    private void OnGlobalUp()
    {
        if (!_mapMoving) return;
        
        _mapMoving = false;
        StateHasChanged();
    }

    private void OnMapPropertyServiceUpdate()
    {
        InvokeAsync(StateHasChanged);
    }
}