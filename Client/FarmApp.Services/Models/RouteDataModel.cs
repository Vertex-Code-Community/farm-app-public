namespace FarmApp.Services.Models;

public class RouteDataModel
{
    public Type PageType { get; set; }
    public string Route { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}