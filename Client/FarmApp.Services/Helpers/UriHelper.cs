namespace FarmApp.Services.Helpers;

public static class UriHelper
{
    public static string Normalize(string? route)
    {
        if (string.IsNullOrEmpty(route)) return "";

        if (route.IndexOf("/", StringComparison.Ordinal) == 0) route = route.Substring(1);

        var lastIndexOfSlash = route.LastIndexOf("/", StringComparison.Ordinal);
        if (lastIndexOfSlash != -1 && lastIndexOfSlash == route.Length - 1) route = route.Substring(0, route.Length - 1);
        
        return route;
    }
}