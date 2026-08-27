namespace FarmApp.DataConverter.Services.Interfaces;

public interface ICsvGenerationService
{
    Task GenerateCsvAsync(string tilesPath, string csvFileName, int districtCode);
}