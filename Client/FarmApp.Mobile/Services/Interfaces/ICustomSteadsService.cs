using FarmApp.ViewModels.CustomSteads;

namespace FarmApp.Mobile.Services.Interfaces;

public interface ICustomSteadsService : IMapService
{
    public const string CustomSteadPolygonsSourceId = "custom-stead-polygons-source";
    public const string CustomSteadPolygonsLayerId = "custom-stead-polygons-id";
    public const string CustomSteadLinesLayerId = "custom-stead-lines-id";
    
    List<CustomSteadModel> CustomSteads { get; set; }
}