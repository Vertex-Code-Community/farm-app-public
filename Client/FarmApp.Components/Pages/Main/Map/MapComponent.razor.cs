using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.CustomSteads;
using FarmApp.ViewModels.Map;
using FarmApp.ViewModels.Properties;

namespace FarmApp.Components.Pages.Main.Map;

public partial class MapComponent
// public partial class MapComponent : IAsyncDisposable
{
    // [Inject] private IJSRuntime JsRuntime { get; set; } = null!;
    // [Inject] private INavigationService NavigationService { get; set; } = null!;
    // [Inject] private IMapSteadService MapSteadService { get; set; } = null!;
    // [Inject] private IMapPropertyService MapPropertyService { get; set; } = null!;
    // [Inject] private ICustomSteadService CustomSteadService { get; set; } = null!;
    // [Inject] private IPropertyService PropertyService { get; set; } = null!;
    // [Inject] private INotificationService NotificationService { get; set; }  = null!;
    // [Inject] private IJSUtilsService JsUtilsService { get; set; } = null!;
    // [Inject] private IStateService StateService { get; set; } = null!;
    // [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    // [Inject] private ISubscriptionStateService SubscriptionStateService { get; set; } = null!;
    //
    // private readonly string _subscriptionKey = $"_key_{Guid.NewGuid()}";
    // private static bool _customSteadsAreLoaded = false;
    //
    // protected override async Task OnInitializedAsync()
    // {
    //     NavigationService.LocationChanged += StateHasChanged;
    //     IMapSteadService.OnUpdate += StateHasChanged;
    //     // IMapCallbackService.OnMapIsLoaded += OnMapIsLoadedAsync;
    //     SubscriptionStateService.OnStateChanged += StateHasChanged;
    //     // ((AuthStateProvider)AuthenticationStateProvider).OnSignInAsync += OnUserSignInAsync;
    //     
    //     await JsUtilsService.AddDocumentListenerAsync<ExtMouseEventArgs>("mouseup", _subscriptionKey, OnMouseUpAsync, preventDefault: false);
    //     await JsUtilsService.AddDocumentListenerAsync<ExtMouseEventArgs>("touchend", _subscriptionKey, OnMouseUpAsync, preventDefault: false);
    // }

    // public async ValueTask DisposeAsync()
    // {
    //     NavigationService.LocationChanged -= StateHasChanged;
    //     IMapSteadService.OnUpdate -= StateHasChanged;
    //     // IMapCallbackService.OnMapIsLoaded -= OnMapIsLoadedAsync;
    //     SubscriptionStateService.OnStateChanged -= StateHasChanged;
    //     // ((AuthStateProvider) AuthenticationStateProvider).OnSignInAsync -= OnUserSignInAsync;
    //     
    //     await JsUtilsService.RemoveDocumentListenerAsync<ExtMouseEventArgs>("mouseup", _subscriptionKey);
    //     await JsUtilsService.RemoveDocumentListenerAsync<ExtMouseEventArgs>("touchend", _subscriptionKey);
    // }

    // protected override async Task OnAfterRenderAsync(bool firstRender)
    // {
    //     if (firstRender) await JsRuntime.InvokeVoidAsync("initMap");
    // }
    //
    // private async Task OnMouseUpAsync(ExtMouseEventArgs e)
    // {
    //     await JsRuntime.InvokeVoidAsync("onPolygonEditorUp");
    // }
    //
    // private Task? _drawCustomSteads;
    //
    // private async Task OnMapIsLoadedAsync()
    // {
    //     if (_customSteadsAreLoaded) return;
    //     _customSteadsAreLoaded = true;
    //
    //     // Print the thread ID
    //     Console.WriteLine($"Thread ID MAP BEFORE: {Thread.CurrentThread.ManagedThreadId}");
    //     
    //     //var customSteads = await CustomSteadService.GetAllOfCurrentUserAsync();
    //     //await MapSteadService.DrawCustomSteadsAsync(customSteads);
    //     
    //     Console.WriteLine($"Thread ID MAP AFTER: {Thread.CurrentThread.ManagedThreadId}");
    //     
    //     //  if (_drawCustomSteads is not null) await _drawCustomSteads;
    //     //
    //     //  _drawCustomSteads = MapSteadService.DrawCustomSteadsAsync(new List<CustomSteadModel>());
    //     //  await _drawCustomSteads;
    //      
    //      Console.WriteLine("OnMapIsLoadedAsync INVOKE");
    //
    //     // var isAuthorized = ((AuthStateProvider)AuthenticationStateProvider).IsUserLoggedIn;
    //     // StateService.PropertiesTask ??= PropertyService.GetAllOfCurrentUserAsync(isAuthorized, isAuthorized);
    //     // var properties = await StateService.PropertiesTask;
    //     // StateService.AddProperties(properties);
    //     //
    //     // await MapPropertyService.DrawPropertiesAsync(properties);
    // }
    //
    // private async Task OnUserSignInAsync(bool signIn)
    // {
    //     StateService.Clear();
    //
    //     if (signIn)
    //     {
    //         _customSteadsAreLoaded = false;
    //         await OnMapIsLoadedAsync();
    //     }
    //     else
    //     {
    //         // IMapSteadService.InvokeDrawCustomSteads(new List<CustomSteadModel>());
    //         // await MapPropertyService.DrawPropertiesAsync(new List<PropertyModel>());
    //     }
    // }
    //
    // private async Task OnCreateCustomSteadAsync()
    // {
    //     // var editorStateModel = await MapSteadService.GetSteadEditorStateAsync();
    //     var editorStateModel = new SteadEditorStateModel();// await MapSteadService.GetSteadEditorStateAsync();
    //     IMapSteadService.InvokeSwitchDrawingMode(false);
    //
    //     CustomSteadModel? customSteadModel = null;
    //     
    //     if (string.IsNullOrWhiteSpace(editorStateModel.CustomSteadId))
    //     {
    //         customSteadModel = await CustomSteadService.CreateAsync(new CreateCustomSteadModel
    //         {
    //             SteadId = editorStateModel.SteadId,
    //             Coordinates = editorStateModel.Coordinates
    //         });
    //     }
    //     else
    //     {
    //         customSteadModel = await CustomSteadService.UpdateAsync(editorStateModel.CustomSteadId, 
    //             new UpdateCustomSteadModel { Coordinates = editorStateModel.Coordinates });
    //     }
    //     
    //     if (customSteadModel is null) return;
    //     
    //     NotificationService.Add("Success", "Custom stead was successfully created!");
    //     IMapSteadService.InvokeDrawCustomStead(customSteadModel);
    // }
    //
    // private async Task OnCreatePropertyAsync()
    // {
    //     // var propertyEditorStateModel = await MapPropertyService.GetPropertyEditorStateAsync();
    //     // await MapPropertyService.SwitchCreateModeAsync(false);
    //     //
    //     // var propertyModel = await PropertyService.CreateAsync(new CreatePropertyModel
    //     // {
    //     //     SteadIds = propertyEditorStateModel.SteadIds,
    //     //     CustomSteadIds = propertyEditorStateModel.CustomReportSteadIds,
    //     //     MultipolygonSerialized = propertyEditorStateModel.MultipolygonSerialized,
    //     //     Area = propertyEditorStateModel.Area
    //     // });
    //     //
    //     // if (propertyModel is null) return;
    //     //
    //     // StateService.AddProperty(propertyModel);
    //     // NotificationService.Add("Success", "Property was successfully created!");
    //     // await MapPropertyService.DrawPropertyAsync(propertyModel);
    // }
}