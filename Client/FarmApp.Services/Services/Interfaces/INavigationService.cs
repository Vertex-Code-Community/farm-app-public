using FarmApp.Services.Models;

namespace FarmApp.Services.Services.Interfaces;

public interface INavigationService
{
    Action? LocationChanged { get; set; }
    List<RouteDataModel> History { get; } 
    
    List<RouteDataModel> Pages { get; }
    RouteDataModel? CurrentPage { get; set; }

    RouteDataModel? GetRouteModelByRoute(string route);
    void NavigateTo(string path, Dictionary<string, object>? parameters = null);
    bool Back();
}