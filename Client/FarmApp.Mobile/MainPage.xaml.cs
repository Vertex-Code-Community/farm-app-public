using Bch.Modules.Maths.Models;
using FarmApp.Mobile.Services;
using FarmApp.Mobile.Services.Interfaces;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Services.Providers;
using FarmApp.Mobile.Bridges;
using FarmApp.Components.Services.Interfaces;
using FarmApp.Components.Services;




#if ANDROID
using Android.Views;
#endif
#if IOS
using CoreGraphics;
using UIKit;
#endif

namespace FarmApp.Mobile;

public partial class MainPage : ContentPage, IDisposable
{
    private readonly IEnumerable<IMapService> _services;
    private readonly MapStyleState _mapStyleState;
    private readonly IBackButtonService _backButtonService;
    private readonly AuthStateProvider _authStateProvider;
    private readonly PushNotificationsRegistrationService _pushNotificationsRegistrationService;
    private readonly IPushNotificationPermissionService _pushNotificationPermissionService;
    private readonly IMapSearchStateService _mapSearchStateService;
    private readonly IGuideTourService _guideTourService;

    private bool _pointerIsDownOnMap;
    private bool _styleInitialized;
    private readonly Vec2 _downPointerPos = new();

    public MainPage(IEnumerable<IMapService> services, MapStyleState mapStyleState,
        AuthStateProvider authStateProvider, PushNotificationsRegistrationService pushNotificationsRegistrationService,
        IPushNotificationPermissionService pushNotificationPermissionService,  IBackButtonService backButtonService,
        IMapSearchStateService mapSearchStateService, IGuideTourService guideTourService)
    {
        _services = services;
        _mapStyleState = mapStyleState;
        _authStateProvider = authStateProvider;
        _pushNotificationsRegistrationService = pushNotificationsRegistrationService;
        _pushNotificationPermissionService = pushNotificationPermissionService;
        _guideTourService = guideTourService;
        
        _backButtonService = backButtonService;
        _mapSearchStateService = mapSearchStateService;
        InitializeComponent();

#pragma warning disable CS0618
        RootLayout.IgnoreSafeArea = true;
#pragma warning restore CS0618

        RootLayout.BlazorView = BlazorWebView;
        RootLayout.MapView = MaplibreView;

        _mapStyleState.StyleChanged += OnMapStyleChanged;
        SystemEventsProvider.OnTouchEvent += OnTouchEvent;
        SystemEventsProvider.OnLastTouchLifted += OnLastTouchLifted;
        IMapCallbackService.OnFlyTo += OnFlyTo;
        IMapCallbackService.OnResetBearing += OnResetBearing;
        IMapCallbackService.OnGetBearing += OnGetBearing;
        MaplibreView.HandlerChanged = MaplibreHandlerInitialized;
        Main.OnBlazorInitialized += OnBlazorLoadedAsync;

#if IOS
        SystemEventsProvider.OnHitTestTouchEvent += OnHitTestTouchEvent;
#endif
        
        _authStateProvider.OnSignInAsync += UpdateDeviceTagsAsync;
        UpdateDeviceTagsAsync(_authStateProvider.IsUserLoggedIn);
    }
    
    private async Task UpdateDeviceTagsAsync(bool isSingIn)
    {
        var status = await _pushNotificationPermissionService.CheckStatusAsync();
        if (status == PermissionStatus.Granted && isSingIn)
            await _pushNotificationsRegistrationService.RegisterDeviceAsync();
    }


    private void MaplibreHandlerInitialized()
    {
        foreach (var service in _services)
        {
            service.MaplibreMapService = MaplibreView.MapService;

            MaplibreView.CallbackService.OnStyleLoaded += service.OnStyleLoaded;
            MaplibreView.CallbackService.OnMapRotate += service.OnMapRotate;
        }

        MaplibreView.CallbackService.OnStyleLoaded += IMapCallbackService.InvokeMapIsLoaded;
    }
    private void OnMapStyleChanged()
    {
        MaplibreView.StyleUrl = _mapStyleState.StyleUrl;
    }

    public void Dispose()
    {
        _mapStyleState.StyleChanged -= OnMapStyleChanged;
        SystemEventsProvider.OnTouchEvent -= OnTouchEvent;
        SystemEventsProvider.OnLastTouchLifted -= OnLastTouchLifted;
        IMapCallbackService.OnFlyTo -= OnFlyTo;
        IMapCallbackService.OnResetBearing -= OnResetBearing;
        IMapCallbackService.OnGetBearing -= OnGetBearing;

#if IOS
        SystemEventsProvider.OnHitTestTouchEvent -= OnHitTestTouchEvent;
#endif

        foreach (var service in _services)
        {
            MaplibreView.CallbackService.OnStyleLoaded -= service.OnStyleLoaded;
            MaplibreView.CallbackService.OnMapRotate -= service.OnMapRotate;
        }

        MaplibreView.CallbackService.OnStyleLoaded -= IMapCallbackService.InvokeMapIsLoaded;
        Main.OnBlazorInitialized -= OnBlazorLoadedAsync;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_styleInitialized)
            return;

        _styleInitialized = true;

        _mapStyleState.Initialize();
        MaplibreView.StyleUrl = _mapStyleState.StyleUrl;
    }
    private async Task OnBlazorLoadedAsync()
    {
        await Task.Delay(300);

        await NativeLoaderOverlay.FadeTo(0, 250);
        NativeLoaderOverlay.IsVisible = false;
    }

    private void OnFlyTo(float lng, float lat, float zoom)
    {
        MaplibreView.MapService.FlyTo(lng, lat, zoom);
    }

    private float OnGetBearing()
    {
        return (float)MaplibreView.MapService.Bearing;
    }

    private void OnResetBearing()
    {
        MaplibreView.MapService.ResetBearing();
    }

    protected override bool OnBackButtonPressed()
    {
        if (_backButtonService != null && _backButtonService.HasSubscribers)
        {
            Dispatcher.Dispatch(async () =>
            {
                bool wasHandledByBlazor = await _backButtonService.RaiseBackButtonPressed();

                if (!wasHandledByBlazor)
                {
                    var navigationService = ServiceLocator.Resolve<INavigationService>();
                    navigationService?.Back();
                }
            });

            return true;
        }

        var navService = ServiceLocator.Resolve<INavigationService>();
        return navService != null && navService.Back();
    }

    private bool OnTouchEvent(object e)
    {
        if (!IMapCallbackService.MapEventsAllowed()) return true;

        var processResult = true;

        var navigationService = ServiceLocator.Resolve<INavigationService>();
        var notificationService = ServiceLocator.Resolve<INotificationService>();
        var mapModalService = ServiceLocator.Resolve<IMapModalService>();

        if (navigationService?.CurrentPage is not { Route: Shared.Constants.Constants.ClientRoutes.MainPage } ||
            notificationService?.Notifications.Count > 0 ||
            mapModalService is null ||
            _mapSearchStateService is null) return true;

        if (_guideTourService.ActiveGroup != TourGroup.None && _guideTourService?.ActiveStep?.TargetClicked == false) return true;

#if ANDROID
        {
                if (e is not MotionEvent motionEvent) return true;

                if (motionEvent.ActionMasked == MotionEventActions.Move)
                {
                    var x = motionEvent.GetX();
                    var y = motionEvent.GetY();

                    if (_pointerIsDownOnMap)
                    {
                        foreach (var service in _services)
                            processResult &= service.OnMapMove(x, y);
                    }
                }

                if (motionEvent.ActionMasked == MotionEventActions.Up)
                {
                    var pointerIsDownOnMap = _pointerIsDownOnMap;
                    _pointerIsDownOnMap = false;

                    var x = motionEvent.GetX();
                    var y = motionEvent.GetY();
                    const float tolerance = 0.01f;

                    foreach (var service in _services)
                        processResult &= service.OnUp(x, y);

                    if (pointerIsDownOnMap && Math.Abs(x - _downPointerPos.X) < tolerance &&
                        Math.Abs(y - _downPointerPos.Y) < tolerance)
                        foreach (var service in _services)
                            processResult &= service.OnClick(x, y);
                }

                if (motionEvent.ActionMasked == MotionEventActions.Down)
                {
                    var x = motionEvent.GetX();
                    var y = motionEvent.GetY();

                    var cssX = x / ScreenOffsetProvider.Density;
                    var cssY = y / ScreenOffsetProvider.Density;

                    if (!(_pointerIsDownOnMap = MapCollisionResolver.IsPointerOnMap(mapModalService, _mapSearchStateService, x, y)))
                        return true;

                    _downPointerPos.X = x;
                    _downPointerPos.Y = y;

                    foreach (var service in _services)
                        processResult &= service.OnDown(x, y);
                }

                if (motionEvent.ActionMasked is MotionEventActions.PointerDown or MotionEventActions.Pointer1Down
                    or MotionEventActions.Pointer2Down or MotionEventActions.Pointer3Down)
                {
                    var x = motionEvent.GetX();
                    var y = motionEvent.GetY();

                    if (motionEvent.PointerCount > 1)
                    {
                        foreach (var service in _services)
                            processResult &= service.OnUp(x, y);
                    }
                }
            }
            
            if (processResult && (!mapModalService.IsShown || _pointerIsDownOnMap))
                MaplibreView.TriggerTouchEvent(e);
#elif IOS
        {
            if (e is not UIEvent uiEvent) return true;

            var anyTouch = uiEvent.AllTouches?.AnyObject as UITouch;
            if (anyTouch is null) return true;

            var touchView = BlazorWebView?.Handler?.PlatformView as UIView;
            var localPoint = anyTouch.LocationInView(touchView);
            
            var x = (float)localPoint.X;
            var y = (float)localPoint.Y;

            switch (anyTouch.Phase)
            {
                case UITouchPhase.Moved:
                    if (_pointerIsDownOnMap)
                    {
                        foreach (var service in _services)
                            processResult &= service.OnMapMove(x, y);
                    }
                    break;

                case UITouchPhase.Ended:
                case UITouchPhase.Cancelled:
                {
                    var pointerWasDown = _pointerIsDownOnMap;
                    _pointerIsDownOnMap = false;

                    foreach (var service in _services)
                        processResult &= service.OnUp(x, y);

                    const float tolerance = 0.8f;
                    if (pointerWasDown &&
                        Math.Abs(x - _downPointerPos.X) < tolerance &&
                        Math.Abs(y - _downPointerPos.Y) < tolerance)
                    {
                        foreach (var service in _services)
                            processResult &= service.OnClick(x, y);
                    }
                    break;
                }

                case UITouchPhase.Began:
                {
                    if (!(_pointerIsDownOnMap = MapCollisionResolver.IsPointerOnMap(mapModalService, _mapSearchStateService, x, y)))
                        return true;
                    _downPointerPos.X = x;
                    _downPointerPos.Y = y;

                    foreach (var service in _services)
                        processResult &= service.OnDown(x, y);
                    break;
                }
            }

            if (uiEvent.AllTouches is { Count: > 1 } && anyTouch.Phase == UITouchPhase.Began)
            {
                foreach (var service in _services)
                    processResult &= service.OnUp(x, y);
            }
        }
#endif
        return processResult;
    }

#if IOS
    private bool OnHitTestTouchEvent(object e)
    {
        if (!IMapCallbackService.MapEventsAllowed()) return true;

        var navigationService = ServiceLocator.Resolve<INavigationService>();
        var notificationService = ServiceLocator.Resolve<INotificationService>();
        var mapModalService = ServiceLocator.Resolve<IMapModalService>();

        if (navigationService?.CurrentPage is not { Route: Shared.Constants.Constants.ClientRoutes.MainPage } ||
            notificationService?.Notifications.Count > 0 ||
            mapModalService is null) return true;

        if (_guideTourService?.ActiveStep != null) return true;

        if (e is not CGPoint uiEvent) return true;

        var x = (float)uiEvent.X;
        var y = (float)uiEvent.Y;

        var pointerOnMap = MapCollisionResolver.IsPointerOnMap(mapModalService, _mapSearchStateService, x, y);
        return !pointerOnMap || mapModalService.IsShown;
    }
#endif

    private void OnLastTouchLifted()
    {
    }
}