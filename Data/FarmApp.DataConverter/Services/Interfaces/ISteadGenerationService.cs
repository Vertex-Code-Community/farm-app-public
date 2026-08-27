using FarmApp.DataConverter.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FarmApp.DataConverter.Services.Interfaces;

public interface ISteadGenerationService
{
    Task GenerateSteadsAsync(List<int> zoomList, List<CsvRowModel> rows, IServiceScope scope, int districtIndex);
}