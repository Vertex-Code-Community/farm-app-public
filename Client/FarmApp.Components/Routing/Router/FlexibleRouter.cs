using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.WebUtilities;
using FarmApp.Services.Helpers;
using FarmApp.Services.Models;
using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Extensions;

namespace FarmApp.Components.Routing.Router;

public class FlexibleRouter : IComponent, IHandleAfterRender, IDisposable
{
    [Inject] public INavigationService NavigationService { get; set; } = null!;
    [Inject] public NavigationManager DefaultNavigationManager { get; set; } = null!;
    [Inject] private INavigationInterception NavigationInterception { get; set; } = null!;
    [Inject] private PageResolveProvider PageResolveProvider { get; set; } = null!;
    
    [Inject] private IMapCallbackService MapCallbackService { get; set; } = null!; // REQUIRED
    [Inject] private IMapPropertyService MapPropertyService { get; set; } = null!; // REQUIRED
    [Inject] private IMapSteadService MapSteadService { get; set; } = null!; // REQUIRED

    [Parameter] public Assembly AppAssembly { get; set; } = null!;
    [Parameter] public IEnumerable<Assembly> AdditionalAssemblies { get; set; } = new List<Assembly>();
    [Parameter] public RenderFragment NotFound { get; set; } = null!;
    [Parameter] public RenderFragment<RouteDataModel> Found { get; set; } = null!;

    private RenderHandle? _renderHandle;
    private bool _navigationInterceptionEnabled;

    public void Attach(RenderHandle renderHandle)
    {
        _renderHandle = renderHandle;

        NavigationService.LocationChanged += HandleLocationChanged;
        DefaultNavigationManager.LocationChanged += HandleLocationChanged;
    }

    public Task OnAfterRenderAsync()
    {
        if (_navigationInterceptionEnabled) return Task.CompletedTask;

        _navigationInterceptionEnabled = true;
        return NavigationInterception.EnableNavigationInterceptionAsync();
    }

    public Task SetParametersAsync(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);

        if (Found == null) throw new InvalidOperationException($"The {nameof(FlexibleRouter)} component requires a value for the parameter {nameof(Found)}.");
        if (NotFound == null) throw new InvalidOperationException($"The {nameof(FlexibleRouter)} component requires a value for the parameter {nameof(NotFound)}.");
        if (AppAssembly == null) throw new InvalidOperationException($"The {nameof(FlexibleRouter)} component requires a value for the parameter {nameof(AppAssembly)}.");

        //var relativePath = DefaultNavigationManager.ToBaseRelativePath(DefaultNavigationManager.Uri);
        if (NavigationService.Pages.Count() > 0)
        {
            return Task.CompletedTask;
        }
        var assemblies = AdditionalAssemblies.ToList();
        assemblies.Add(AppAssembly);
        
        Console.WriteLine($"Assemblies count {assemblies.Count}");

        var pages = assemblies.SelectMany(x =>
                x.GetTypesWithAttribute<RouteAttribute>()
                    .Select(t => new RouteDataModel
                    {
                        PageType = t,
                        Route = UriHelper.Normalize(t.GetValue<string, RouteAttribute>(a => a.Template))
                    }))
            .ToList();

        var indexPageRoute = PageResolveProvider.GetInitialPageRoute();
        var indexPage = pages.FirstOrDefault(x => x.Route == indexPageRoute);

        NavigationService.CurrentPage = indexPage;

        if (indexPage is not null) NavigationService.History.Add(indexPage);

        NavigationService.Pages.AddRange(pages);
        
        RenderContent();
        
        return Task.CompletedTask;
    }
    
    public void Dispose()
    {
        NavigationService.LocationChanged -= HandleLocationChanged;
        DefaultNavigationManager.LocationChanged -= HandleLocationChanged;
    }

    private void HandleLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        var uri = new Uri(args.Location);
        var queryParameters = QueryHelpers.ParseQuery(uri.Query)
            .ToDictionary(kv=> kv.Key, kv => $"{kv.Value}" as object);
        
        NavigationService.NavigateTo(uri.LocalPath, queryParameters);
    }

    private void HandleLocationChanged()
    {
        // Console.WriteLine("CUSTOM HandleLocationChanged");
        RenderContent();
    }

    private void RenderContent()
    {
        // Console.WriteLine($"RenderContent {_renderHandle is null}");
        _renderHandle?.Render(NavigationService.CurrentPage is null
            ? NotFound
            : Found(NavigationService.CurrentPage));
    }
}