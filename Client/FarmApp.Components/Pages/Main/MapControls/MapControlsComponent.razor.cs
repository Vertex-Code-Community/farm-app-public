using FarmApp.Components.Helpers;
using FarmApp.Components.Services;
using FarmApp.Components.Services.Interfaces;
using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Enums;
using FarmApp.Shared.Helpers;
using FarmApp.Shared.Math;
using FarmApp.ViewModels.CustomSteads;
using FarmApp.ViewModels.Map;
using FarmApp.ViewModels.Properties;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System.Globalization;
using System.Threading;

namespace FarmApp.Components.Pages.Main.MapControls;

public partial class MapControlsComponent : IDisposable
{
    [Inject] public required IStateService StateService { get; set; }
    [Inject] public required ICustomSteadService CustomSteadService { get; set; }
    [Inject] public required IPropertyService PropertyService { get; set; }
    [Inject] public required INotificationService NotificationService { get; set; }
    [Inject] public required AuthStateProvider AuthStateProvider { get; set; }
    [Inject] public required ILocationService LocationService { get; set; }
    [Inject] public required IGlobalLoaderService GlobalLoaderService { get; set; }
    [Inject] public required IJSRuntime JsRuntime { get; set; }
    [Inject] public required ICountryMapStorage CountryMapStorage { get; set; }
    [Inject] public required IPlatformContext PlatformContext { get; set; }
    [Inject] public required IMapControlsVisibilityService Visibility { get; set; }

    [Inject] public IGuideTourService GuideTourService { get; set; } = default!;

    [Parameter] public bool ShowCreateFieldHint { get; set; } = false;

    private readonly NumberFormatInfo _nF = new() { NumberDecimalSeparator = "." };
    private string _compassRotation = string.Empty;
    private readonly VariableChangeTracker<float> _bearingTracker = new(8, 10);
    private static Task<Vec2?>? _getLocationTask;

    private bool _showPropertyCreateModal;

    /// <summary>Bound to create-field sheet; when true, tab bar and map creation controls are hidden.</summary>
    public bool ShowPropertyCreateModal
    {
        get => _showPropertyCreateModal;
        set
        {
            if (_showPropertyCreateModal == value) return;
            _showPropertyCreateModal = value;
            IMapPropertyService.SetFieldCreateModalOpen(value);
        }
    }
    private string _propertyName = string.Empty;
    private string _propertyAreaText = string.Empty;
    private int _createPropertyInFlight;
    private bool _animationPlayed = false;

    private bool showCompass = true;

    private const int CreateButtonRestoreDelayMs = 400;
    private bool _createButtonHidden;
    private CancellationTokenSource? _createButtonRestoreCts;

    private bool ShowPropertyConfirmButton =>
        IMapPropertyService.IsCreationModeOn && PropertyCreationHasParcelSelection;

    private bool PropertyCreationHasParcelSelection =>
        IMapPropertyService.HasPendingDrawnCustomSteadForProperty
        || (OperatingSystem.IsBrowser()
            ? IMapPropertyService.WebPropertyParcelSelectionCount > 0
            : IStateService.SteadIds.Count + IStateService.CustomSteadIds.Count > 0);

    private float? CurrentPropertyArea
    {
        get
        {
            if (OperatingSystem.IsBrowser()) return null;
            if (IMapSteadService.IsDrawingModeOn)
                return ComputeDrawingAreaHa(IMapSteadService.SteadEditorState?.Coordinates);
            return IMapPropertyService.PropertyEditorState?.Area;
        }
    }

    private bool IsCurrentPropertyAreaOverLimit =>
        CurrentPropertyArea is float a && a > Constants.PropertyLimits.MaxFieldAreaHectares;

    private string CurrentPropertyAreaLabel =>
        string.Format(
            CultureInfo.InvariantCulture,
            "{0:0.##} / {1:0.##} га",
            CurrentPropertyArea ?? 0f,
            Constants.PropertyLimits.MaxFieldAreaHectares);

    private static float ComputeDrawingAreaHa(string? coordinatesJson)
    {
        if (string.IsNullOrWhiteSpace(coordinatesJson) || coordinatesJson == "[]") return 0f;
        try
        {
            var ring = JsonConvert.DeserializeObject<double[][]>(coordinatesJson);
            if (ring is null || ring.Length < 4) return 0f;
            var coords = NetTopologySuiteUtils.ConvertToCoordinates(ring);
            return (float)(NetTopologySuiteUtils.GetAreaOfPolygon(coords) / 10000d);
        }
        catch
        {
            return 0f;
        }
    }

    protected override void OnInitialized()
    {
        IMapSteadService.OnUpdate += StateHasChanged;
        IMapPropertyService.OnUpdate += StateHasChanged;
        IMapCallbackService.OnRotate += OnMapRotate;
        IMapCallbackService.OnRequestMapEventPermission += AllowMapEvents;
        IMapSteadService.OnDrawingModeSwitch += OnDrawingModeSwitch;
        GuideTourService.OnTourStateChanged += OnTourStateChanged;
        Visibility.Changed += StateHasChanged;
        _bearingTracker.OnGetValue += GetBearing;
        _bearingTracker.OnChange += OnBearingUpdate;

        _compassRotation = (-IMapCallbackService.Bearing - 44.04).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
        _createButtonHidden = IMapSteadService.IsDrawingModeOn;

        if (GuideTourService.ActiveGroup == TourGroup.HowToUse) ToggleSteadsBlockGuide();
    }

    private void OnTourStateChanged() => InvokeAsync(StateHasChanged);

    private void OnDrawingModeSwitch()
    {
        _createButtonRestoreCts?.Cancel();

        if (IMapSteadService.IsDrawingModeOn)
        {
            _createButtonHidden = true;
            return;
        }

        var cts = new CancellationTokenSource();
        _createButtonRestoreCts = cts;
        _ = InvokeAsync(async () =>
        {
            try
            {
                await Task.Delay(CreateButtonRestoreDelayMs, cts.Token);
                _createButtonHidden = false;
                StateHasChanged();
            }
            catch (TaskCanceledException) { }
        });
    }

    public void Dispose()
    {
        IMapSteadService.OnUpdate -= StateHasChanged;
        IMapPropertyService.OnUpdate -= StateHasChanged;
        IMapCallbackService.OnRotate -= OnMapRotate;
        IMapCallbackService.OnRequestMapEventPermission -= AllowMapEvents;
        IMapSteadService.OnDrawingModeSwitch -= OnDrawingModeSwitch;
        GuideTourService.OnTourStateChanged -= OnTourStateChanged;
        Visibility.Changed -= StateHasChanged;
        _createButtonRestoreCts?.Cancel();
        _createButtonRestoreCts?.Dispose();
        _bearingTracker.OnGetValue -= GetBearing;
        _bearingTracker.OnChange -= OnBearingUpdate;

        IMapPropertyService.SetFieldCreateModalOpen(false);
        _bearingTracker.Dispose();
    }

    private float GetBearing() => IMapCallbackService.Bearing;

    private Task OnMapRotate(float _)
    {
        return _bearingTracker.TrackAsync();
    }

    private void OnBearingUpdate()
    {
        _compassRotation = (-IMapCallbackService.Bearing - 44.04).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
        StateHasChanged();
    }

    private async Task OnSaveSteadEditorClicked()
    {
        var editorStateModel = IMapSteadService.SteadEditorState;

        var steadSelected = editorStateModel?.SteadId is not null || editorStateModel?.CustomSteadId is not null;
        var coordinatesEmpty = string.IsNullOrEmpty(editorStateModel?.Coordinates) || editorStateModel.Coordinates is "[]";

        Console.WriteLine($"OnSaveSteadEditorClicked {DebugHelper.SingleLineJsonToMultiline(JsonConvert.SerializeObject(editorStateModel))}");

        if (editorStateModel is null || (!steadSelected && coordinatesEmpty))
        {
            NotificationService.Add("Помилка", "Додайте ділянку натиснувши на вільне місце на карті");
            return;
        }

        if (!coordinatesEmpty
            && ComputeDrawingAreaHa(editorStateModel.Coordinates) > Constants.PropertyLimits.MaxFieldAreaHectares)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(editorStateModel.CustomSteadId))
        {
            IMapPropertyService.PendingDrawnCustomSteadCoordinates = editorStateModel.Coordinates;
            IMapPropertyService.NotifySelectionChanged();

            IMapSteadService.InvokeSwitchDrawingMode(false);

            IMapPropertyService.InvokeSwitchCreationMode(true);
            if (OperatingSystem.IsBrowser())
            {
                try
                {
                    await JsRuntime.InvokeVoidAsync("farmAppSetPendingDrawnPolygonForProperty", editorStateModel.Coordinates);
                    await JsRuntime.InvokeVoidAsync("farmAppMapStartPropertyCreating");
                }
                catch (JSException)
                {
                    IMapPropertyService.ClearPendingDrawnCustomSteadForProperty();
                    IMapPropertyService.InvokeSwitchCreationMode(false);
                    NotificationService.Add("Помилка", "Не вдалося підготувати створення поля на карті");
                    return;
                }
            }

            await TryOpenPropertyNameModalAsync(requireCreationMode: false);
            return;
        }

        using var loader = GlobalLoaderService.SwitchOn();

        IMapSteadService.InvokeSwitchDrawingMode(false);

        var customSteadModel = await CustomSteadService.UpdateAsync(editorStateModel.CustomSteadId,
            new UpdateCustomSteadModel { Coordinates = editorStateModel.Coordinates });

        IMapSteadService.InvokeRemoveCustomStead(editorStateModel.CustomSteadId);
        NotificationService.Add("Результат", "Ділянка додана успішно");

        if (customSteadModel is null) return;
        IMapSteadService.InvokeDrawCustomStead(customSteadModel);
    }

    private async Task OnCreatePropertyAsync()
    {
        ShowPropertyCreateModal = false;
        StateHasChanged();
        await Task.Delay(1);

        if (string.IsNullOrWhiteSpace(_propertyName))
        {
            _propertyName = string.Empty;
            _propertyAreaText = string.Empty;
            ShowPropertyCreateModal = false;
            NotificationService.Add("Помилка введення даних", "Імʼя не повинно бути пустим");
            return;
        }

        if (Interlocked.CompareExchange(ref _createPropertyInFlight, 1, 0) != 0)
            return;

        try
        {
            using var loader = GlobalLoaderService.SwitchOn();

            var state = await ResolvePropertyEditorStateAsync();

            if (state is null)
            {
                _propertyName = string.Empty;
                _propertyAreaText = string.Empty;
                ShowPropertyCreateModal = false;
                NotificationService.Add("Помилка", "Не вдалося зібрати геометрію поля. Виберіть ділянки й спробуйте ще раз.");
                await StopPropertyCreationUiAsync();
                return;
            }

            // Не викликати StopPropertyCreationUiAsync тут: виділення й pending-контур лишаються до успішного CreateAsync.

            string? featureJson;
            if (state.Features.Count > 0)
            {
                featureJson = await JsRuntime.InvokeAsync<string?>("combineGeometriesFromSerializedFeatures", state.Features);
            }
            else if (!string.IsNullOrWhiteSpace(state.MultipolygonSerialized))
            {
                featureJson = state.MultipolygonSerialized;
            }
            else
            {
                featureJson = null;
            }

            if (string.IsNullOrEmpty(featureJson))
            {
                _propertyName = string.Empty;
                _propertyAreaText = string.Empty;
                ShowPropertyCreateModal = false;
                NotificationService.Add("Помилка", "Не вдалося об’єднати геометрію поля");
                return;
            }

            var areaForCreate = state.Area;
            if (TryParseUserAreaHa(_propertyAreaText, out var parsedArea))
                areaForCreate = parsedArea;

            if (areaForCreate > Constants.PropertyLimits.MaxFieldAreaHectares)
            {
                _propertyName = string.Empty;
                _propertyAreaText = string.Empty;
                ShowPropertyCreateModal = false;
                NotificationService.Add(
                    "Помилка",
                    $"Площа поля не може перевищувати {Constants.PropertyLimits.MaxFieldAreaHectares:0.##} га. Поточна: {areaForCreate:0.##} га.");
                await StopPropertyCreationUiAsync();
                return;
            }

            PropertyViewModel? propertyModel;
            try
            {
                propertyModel = await PropertyService.CreateAsync(new CreatePropertyModel
                {
                    SteadIds = state.SteadIds,
                    CustomSteadIds = state.CustomSteadIds,
                    MultipolygonSerialized = featureJson,
                    Name = _propertyName,
                    Area = areaForCreate
                });
            }
            catch (Exception ex)
            {
                _propertyName = string.Empty;
                _propertyAreaText = string.Empty;
                NotificationService.Add("Помилка", ex.Message);
                return;
            }

            if (propertyModel is null)
            {
                _propertyName = string.Empty;
                _propertyAreaText = string.Empty;
                ShowPropertyCreateModal = false;
                return;
            }

            await StateService.AddPropertyAsync(propertyModel);
            NotificationService.Add("Результат", "Поле додано успішно");
            IMapPropertyService.InvokeDrawProperty(propertyModel);

            if (GuideTourService.ActiveGroup == TourGroup.Onboarding && GuideTourService.ActiveStep?.Sequence == 2)
            {
                GuideTourService.CompleteCurrentStep();
            }

            await StopPropertyCreationUiAsync();

            _propertyName = string.Empty;
            _propertyAreaText = string.Empty;
            ShowPropertyCreateModal = false;
        }
        finally
        {
            Interlocked.Exchange(ref _createPropertyInFlight, 0);
        }
    }

    private async Task<PropertyEditorStateModel?> ResolvePropertyEditorStateAsync()
    {
        if (OperatingSystem.IsBrowser())
        {
            try
            {
                return await JsRuntime.InvokeAsync<PropertyEditorStateModel?>("farmAppMapGetPropertyEditorStateForCreate");
            }
            catch (JSException)
            {
                return null;
            }
        }

        return IMapPropertyService.PropertyEditorState;
    }

    private async Task StopPropertyCreationUiAsync()
    {
        IMapPropertyService.ClearPendingDrawnCustomSteadForProperty();
        IMapPropertyService.InvokeSwitchCreationMode(false);
        if (OperatingSystem.IsBrowser())
        {
            try
            {
                await JsRuntime.InvokeVoidAsync("farmAppClearPendingDrawnPolygonForProperty");
                await JsRuntime.InvokeVoidAsync("farmAppMapStopPropertyCreating");
            }
            catch (JSException)
            {
                // ignored
            }
        }
    }

    private async Task OnCompassClickedAsync()
    {
        IMapCallbackService.ClearSelection();
        IMapCallbackService.ResetBearing();
        IMapSteadService.InvokeSwitchDrawingMode(false);
        await StopPropertyCreationUiAsync();

        await _bearingTracker.TrackAsync();
    }

    private async Task OnUserLocationClickedAsync()
    {
        IMapCallbackService.ClearSelection();
        IMapSteadService.InvokeSwitchDrawingMode(false);
        await StopPropertyCreationUiAsync();

        if (_getLocationTask is not null) return;
        _getLocationTask = GetUserLocationAsync();
        StateHasChanged();

        var location = await _getLocationTask;
        _getLocationTask = null;
        StateHasChanged();

        if (location is null) return;

        IMapCallbackService.FlyTo((float)location.X, (float)location.Y, 14);
    }

    private async Task OnFlyToSelectedCountryClickedAsync()
    {
        var country = CountryMapStorage.Get() ?? AppMapCountry.Ukraine;
        var (lng, lat, z) = CountryMapViewDefaults.Get(country);

        if (PlatformContext.isWeb)
        {
            try
            {
                await JsRuntime.InvokeVoidAsync("farmAppMapApplyView", lng, lat, z);
            }
            catch (JSException)
            {
                // Web map not initialized yet; ignore.
            }
        }
        else
        {
            IMapCallbackService.FlyTo(lng, lat, z);
        }
    }

    private async Task<Vec2?> GetUserLocationAsync()
    {
        var permissionGranted = await LocationService.RequestPermissionAsync();
        if (!permissionGranted)
        {
            NotificationService.Add("Помилка", "Доступ не надано");
            return null;
        }

        var minimalTimeTask = Task.Delay(TimeSpan.FromMilliseconds(250));
        var locationTask = LocationService.GetUserLocationAsync();
        await Task.WhenAll(locationTask, minimalTimeTask);

        var location = locationTask.Result;
        if (location is not null) return location;

        NotificationService.Add("Помилка", "Не вийшло отримати геолокацію");
        return null;
    }

    private async Task OpenCreatePropertyModalAsync()
    {
        await TryOpenPropertyNameModalAsync(requireCreationMode: true);
    }

    /// <summary>
    /// Opens the field name modal when creation mode is on (if required) and at least one parcel is selected.
    /// </summary>
    /// <returns>True if the modal was shown.</returns>
    private async Task<bool> TryOpenPropertyNameModalAsync(bool requireCreationMode)
    {
        if (requireCreationMode && !IMapPropertyService.IsCreationModeOn)
        {
            NotificationService.Add("Підказка", "Спочатку натисніть кнопку з листком, щоб увімкнути вибір ділянок для поля");
            return false;
        }

        if (OperatingSystem.IsBrowser())
        {
            try
            {
                var count = await JsRuntime.InvokeAsync<int>("farmAppMapGetPropertySelectionCount");
                if (count == 0 && !IMapPropertyService.HasPendingDrawnCustomSteadForProperty)
                {
                    NotificationService.Add("Помилка", "Виберіть одну або кілька ділянок на карті");
                    return false;
                }
            }
            catch (JSException)
            {
                NotificationService.Add("Помилка", "Не вдалося перевірити вибір на карті");
                return false;
            }
        }
        else if (IStateService.SteadIds.Count is 0 && IStateService.CustomSteadIds.Count is 0
                 && !IMapPropertyService.HasPendingDrawnCustomSteadForProperty)
        {
            NotificationService.Add("Помилка", "Виберіть одну або кілька ділянок на карті");
            return false;
        }

        _propertyName = string.Empty;
        _propertyAreaText = string.Empty;
        var previewState = await ResolvePropertyEditorStateAsync();

        // Silently block modal when over limit — user already sees red counter + red confirm button.
        if (previewState is { Area: var area } && area > Constants.PropertyLimits.MaxFieldAreaHectares)
            return false;

        if (previewState is { Area: > 0 })
            _propertyAreaText = previewState.Area.ToString("0.##", CultureInfo.InvariantCulture);

        ShowPropertyCreateModal = true;
        StateHasChanged();
        return true;
    }

    private static bool TryParseUserAreaHa(string? input, out float area)
    {
        area = 0;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var s = input.Trim();
        foreach (var suffix in new[] { "га", "гa", "ha", "hа" })
        {
            if (s.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                s = s[..^suffix.Length].Trim();
                break;
            }
        }

        s = s.Replace(',', '.');
        return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out area) && area >= 0;
    }

    private async Task OnTogglePropertyCreationModeAsync()
    {
        if (IMapPropertyService.IsCreationModeOn)
        {
            await StopPropertyCreationUiAsync();
        }
        else
        {
            IMapSteadService.InvokeSwitchDrawingMode(false);
            IMapPropertyService.InvokeSwitchCreationMode(true);
            if (OperatingSystem.IsBrowser())
            {
                try
                {
                    await JsRuntime.InvokeVoidAsync("farmAppMapStartPropertyCreating");
                }
                catch (JSException)
                {
                    IMapPropertyService.InvokeSwitchCreationMode(false);
                    NotificationService.Add("Помилка", "Не вдалося увімкнути режим створення поля на карті");
                    return;
                }
            }
        }

        StateHasChanged();
    }

    private async Task OnCreateButtonClickedAsync()
    {
        _animationPlayed = true;

        // У режимі вибору ділянок для поля модалку відкриває лише галочка підтвердження; + лишається для малювання ділянки.
        if (IMapPropertyService.IsCreationModeOn)
            return;

        await StopPropertyCreationUiAsync();

        ToggleSteadsBlockGuide();
    }

    private void ToggleSteadsBlockGuide()
    {
        bool targetMode = GuideTourService.ActiveGroup == TourGroup.HowToUse || !IMapSteadService.IsDrawingModeOn;

        if (IMapSteadService.IsDrawingModeOn != targetMode)
        {
            IMapSteadService.InvokeSwitchDrawingMode(targetMode);
        }
    }

    private bool AllowMapEvents()
    {
        return !ShowPropertyCreateModal;
    }
}