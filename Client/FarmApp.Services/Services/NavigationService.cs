using Microsoft.JSInterop;
using Microsoft.AspNetCore.WebUtilities;
using FarmApp.Services.Helpers;
using FarmApp.Services.Models;
using FarmApp.Services.Services.Interfaces;
using Newtonsoft.Json;

namespace FarmApp.Services.Services;

public class NavigationService : INavigationService, IDisposable
{
    public Action? LocationChanged { get; set; }
    public List<RouteDataModel> History { get; } = new();
    public List<RouteDataModel> Pages { get; } = new();
    public RouteDataModel? CurrentPage { get; set; }

    private static Action<string, Dictionary<string, object>>? _jsNavigation;

    public NavigationService()
    {
        _jsNavigation += NavigateTo;
    }
    
    public void Dispose()
    {
        _jsNavigation -= NavigateTo;
    }

    public RouteDataModel? GetRouteModelByRoute(string route)
    {
        return Pages.FirstOrDefault(x => x.Route == UriHelper.Normalize(route));
    }

    public void NavigateTo(string path, Dictionary<string, object>? parameters = null)
    {
        var normalizedPath = UriHelper.Normalize(path);
        
        var page = Pages.FirstOrDefault(x => x.Route == normalizedPath);
        if (page is null || !IsAllowedRoute(page)) return;

        var lastPage = History.LastOrDefault();
        if (lastPage is not null && lastPage.Route == page.Route)
        {
            CurrentPage = new RouteDataModel
            {
                PageType = page.PageType,
                Route = page.Route,
                Parameters = parameters ?? new()
            };
            History[^1] = CurrentPage;
            LocationChanged?.Invoke();
            return;
        }
        
        CurrentPage = new RouteDataModel
        {
            PageType = page.PageType,
            Route = page.Route,
            Parameters = parameters ?? new()
        };
        
        History.Add(CurrentPage);
        LocationChanged?.Invoke();
    }

    public bool Back()
    {
        if (History.Count <= 1) return false;
        History.RemoveAt(History.Count - 1);
        
        var lastItem = History.LastOrDefault();
        if (lastItem is null) return false;
        
        CurrentPage = lastItem;
        LocationChanged?.Invoke();
        
        return true;
    }

    protected virtual bool IsAllowedRoute(RouteDataModel route)
    {
        return true;
    }

    [JSInvokable] public static void NavigateToFromJs(string route)
    {
        var parts = route.Split("?");
        
        var queryParameters = QueryHelpers.ParseQuery(parts.ElementAtOrDefault(1))
            .ToDictionary(kv=> kv.Key, kv => $"{kv.Value}" as object);
        
        _jsNavigation?.Invoke(parts.First(), queryParameters);
    }
}