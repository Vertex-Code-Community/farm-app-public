using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CsvHelper;
using CsvHelper.Configuration;
using FarmApp.DataConverter.Models;
using FarmApp.DataConverter.Services.Interfaces;
using FarmApp.ViewModels.Options;

namespace FarmApp.DataConverter;

public class WorkerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ITileGenerationService _tileGenerationService;
    private readonly IFileGenerationService _fileGenerationService;
    private readonly ITileDownloadingService _tileDownloadingService;
    private readonly ICsvGenerationService _csvGenerationService;

    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Житомирська область.csv";         0
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Місто Севастополь.csv";           1
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Вінницька область.csv";           2
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Харківська область.csv";          3
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Закарпатська область.csv";        4
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Дніпропетровська область.csv";    5
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Хмельницька область.csv";         6
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Волинська область.csv";           7
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Автономна Республіка Крим.csv";   8
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Івано-Франківська область.csv";   9
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Київська область.csv";           10
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Кіровоградська область.csv";     11
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Львівська область.csv";          12
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Миколаївська область.csv";       13
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Одеська область.csv";            14
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Полтавська область.csv";         15
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Рівненська область.csv";         16
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Сумська область.csv";            17
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Тернопільська область.csv";      18
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Черкаська область.csv";          19
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Чернівецька область.csv";        20
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Місто Київ.csv";                 21
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Чернігівська область.csv";       22
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Херсонська область.csv";         23
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Запорізька область.csv";         24
    // private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Донецька область.csv";           25
    private const string FileName = "D://Projects/Web-Svit/FarmApp-data/Луганська область.csv";
    
    private const int _districtIndex = 26;
    private readonly List<int> _zoomList = new() { 14, 13, 12, 11 };

    public WorkerService(IServiceScopeFactory scopeFactory, ITileGenerationService tileGenerationService,
        IFileGenerationService fileGenerationService, ITileDownloadingService tileDownloadingService, 
        ICsvGenerationService csvGenerationService)
    {
        _scopeFactory = scopeFactory;
        _tileGenerationService = tileGenerationService;
        _fileGenerationService = fileGenerationService;
        _tileDownloadingService = tileDownloadingService;
        _csvGenerationService = csvGenerationService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // await DownloadMissingTilesAsync();
            await GenerateCsvFileAsync();

            // await GenerateTilesAsync();
            // await GenerateSteadsAsync();
            await GenerateFilesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }

    private async Task GenerateTilesAsync()
    {
        Console.WriteLine("Tile generation started...");

        foreach (var zoom in _zoomList)
            await _tileGenerationService.GenerateTilesAsync(zoom);
        
        Console.WriteLine("Tile generation finished.");
    }

    private async Task GenerateSteadsAsync()
    {
        Console.WriteLine("Stead generation started...");

        var tasks = new List<Task>();

        using (var reader = new StreamReader(FileName))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            var counter = 0;
            var rowsList = new List<CsvRowModel>();

            while (await csv.ReadAsync())
            {
                var rowModel = csv.GetRecord<CsvRowModel>();
                if (rowModel is null) continue;

                rowsList.Add(rowModel);

                counter++;

                if (counter % 1000 == 0)
                {
                    var scope = _scopeFactory.CreateScope();
                    var steadGenerationService = scope.ServiceProvider.GetRequiredService<ISteadGenerationService>();

                    tasks.Add(steadGenerationService.GenerateSteadsAsync(_zoomList, rowsList, scope, _districtIndex));
                    rowsList = new List<CsvRowModel>();

                    Console.WriteLine($"ThreadPool.ThreadCount {ThreadPool.ThreadCount}");
                    Console.WriteLine($"Counter = {counter}");

                    if (tasks.Count > 10)
                    {
                        await Task.WhenAll(tasks);
                        tasks.Clear();
                    }
                    
                    Console.WriteLine($"PROCESSED = {counter}");
                }
            }

            var scope2 = _scopeFactory.CreateScope();
            var steadGenerationService2 = scope2.ServiceProvider.GetRequiredService<ISteadGenerationService>();

            Console.WriteLine($"ThreadPool.ThreadCount {ThreadPool.ThreadCount}");
            Console.WriteLine($"Counter = {counter}");

            await steadGenerationService2.GenerateSteadsAsync(_zoomList, rowsList, scope2, _districtIndex);
        }

        Console.WriteLine("Stead generation finished.");
    }
    
    private async Task GenerateFilesAsync()
    {
        Console.WriteLine("Mvt file generation started...");

        for (var i = 0; i < _zoomList.Count; i ++)
            await _fileGenerationService.GenerateMvtFilesAsync(_zoomList[i], i);
        
        Console.WriteLine("Mvt file generation finished.");
    }

    private async Task DownloadMissingTilesAsync()
    {
        // Zaporizhzhia, 23
        // UpperLeftLatitude = 48.140534,
        // UpperLeftLongitude = 34.133490,
        // LowerRightLatitude = 46.264199,
        // LowerRightLongitude = 37.245063
        
        // Kherson, 65
        // UpperLeftLatitude = 47.607009,
        // UpperLeftLongitude = 31.513291,
        // LowerRightLatitude = 45.756019,
        // LowerRightLongitude = 35.282851
        
        // Donetsk, 14
        // UpperLeftLatitude = 49.236758,
        // UpperLeftLongitude = 36.540274,
        // LowerRightLatitude = 46.867357,
        // LowerRightLongitude = 39.092202
        
        // Luhansk, 44
        // UpperLeftLatitude = 50.088504,
        // UpperLeftLongitude = 37.837215,
        // LowerRightLatitude = 47.824406,
        // LowerRightLongitude = 40.227838
        
        foreach (var zoom in _zoomList)
        {
            await _tileDownloadingService.DownloadTilesForRegionAsync(new TileGenerationBoundariesOptions
            {
                UpperLeftLatitude = 50.088504,
                UpperLeftLongitude = 37.837215,
                LowerRightLatitude = 47.824406,
                LowerRightLongitude = 40.227838
            }, zoom);
        }
    }

    private async Task GenerateCsvFileAsync()
    {
        await _csvGenerationService.GenerateCsvAsync(
            "/Users/ruslandudchenko/Documents/LuhanskOblast/", "LuhanskOblast.csv", 44);
    }
}