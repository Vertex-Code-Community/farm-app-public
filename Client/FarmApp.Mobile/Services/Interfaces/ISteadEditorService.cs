namespace FarmApp.Mobile.Services.Interfaces;

public interface ISteadEditorService : IMapService
{
    void StartDrawingWithCoordinates(List<double[]> coordinates, string? steadId, string? customSteadId);
}