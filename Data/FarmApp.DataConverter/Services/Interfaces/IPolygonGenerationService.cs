namespace FarmApp.DataConverter.Services.Interfaces;

public interface IPolygonGenerationService
{
    Task GeneratePolygonAsync(string steadId, string wktLine, List<int> zoomList);
}