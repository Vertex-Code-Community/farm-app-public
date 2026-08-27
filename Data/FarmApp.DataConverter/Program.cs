using FarmApp.DataAccessLayer;
using FarmApp.DataConverter.DbContext;
using FarmApp.DataConverter.Services;
using FarmApp.DataConverter.Services.Interfaces;
using FarmApp.MVTParser;
using FarmApp.Shared.Constants;
using FarmApp.ViewModels.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FarmApp.DataConverter;

public class Program
{
    public static void Main(string[] args)
        // public static async Task Main(string[] args)
    {
        // var configurationBuilder = new ConfigurationBuilder()
        //     .SetBasePath(Directory.GetCurrentDirectory())
        //     .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        //
        // var configuration = configurationBuilder.Build();
        // var services = new ServiceCollection();
        //
        // services.AddSingleton(configuration);
        // services.AddScoped<ITileGenerationService, TileGenerationService>();
        // services.AddScoped<ISteadGenerationService, SteadGenerationService>();
        // services.AddScoped<IPolygonGenerationService, PolygonGenerationService>();
        // services.AddScoped<IFileGenerationService, FileGenerationService>();
        //
        // services.Configure<TileGenerationBoundariesOptions>(
        //     configuration.GetSection(Constants.AppSettings.TILE_GENERATION_BOUNDARIES_OPTIONS));
        // services.Configure<TileMvtFileOptions>(
        //     configuration.GetSection(Constants.AppSettings.TILE_MVT_FILE_OPTIONS));
        //
        // services.DataAccessInitializer(configuration);
        //
        // var converterSqlServerConnection =
        //     configuration.GetConnectionString(Constants.AppSettings.CONVERTER_SQL_SERVER_CONNECTION);
        //
        // services.AddDbContext<DataConverterDbContext>(options =>
        //     options.UseNpgsql(converterSqlServerConnection, x => x.MigrationsAssembly("FarmApp.DataConverter")));
        //
        // var serviceProvider = services.BuildServiceProvider();
        // var polygonGenerationService = serviceProvider.GetRequiredService<IPolygonGenerationService>();
        //
        // var wktLine =
        //     "MULTIPOLYGON(((34.8566766911088 49.9519755356145,34.8568563541656 49.9514582205736,34.8569057615063 49.9489900600249,34.8588191730614 49.9471085084162,34.8597489293805 49.9469032963454,34.8599330840137 49.9467558899521,34.8593222296205 49.9466980834002,34.8585811195111 49.9471460823626,34.8571932223972 49.9471403017574,34.8566497416503 49.9478888843634,34.8561781261261 49.9476865658874,34.8554459991696 49.9465824700884,34.8556795611434 49.9464292820228,34.8567844889429 49.946316558418,34.8567395731787 49.9458627709818,34.8563532976065 49.9457240325684,34.8543275966408 49.9452153216337,34.8522929125223 49.9457066902386,34.8514619708845 49.945617088102,34.8513856140853 49.9454928009914,34.8507747596921 49.9454725681755,34.8503076357444 49.9457558268234,34.8498629696788 49.9457500460513,34.8488029576435 49.9453222669921,34.8472937879662 49.9450158823581,34.8468850545119 49.9450968143376,34.8467413240664 49.9453135957557,34.8452725785769 49.9468368190082,34.835395602028 49.9440302355531,34.8341694016652 49.9458396479407,34.8437813752053 49.9487386126824,34.8421060172004 49.9516576331749,34.8425327169603 49.9516836434533,34.8447111315243 49.9518310347657,34.8462292843545 49.9519350754205,34.8483897326128 49.9520246658043,34.8507927259978 49.9520911359813,34.8548755689641 49.9521489360607,34.8566766911088 49.9519755356145),(34.8555807464622 49.9510622829614,34.8550507404445 49.9512877076944,34.8541569167368 49.9512327966387,34.8530025815968 49.9511952258803,34.8522839293695 49.9511171942115,34.851520361378 49.9511547650308,34.8510981531944 49.9510102617194,34.8516146844828 49.9507963960227,34.8526163060246 49.9507906158555,34.8533933487453 49.9508859885247,34.8549564173397 49.9510276021396,34.8555852380386 49.9502992991127,34.8560388872571 49.9506172105186,34.8555807464622 49.9510622829614),(34.8536763180598 49.9478166278624,34.8537077590948 49.9479727017688,34.8521312157712 49.9479004453935,34.8515248529544 49.9482732871265,34.8511071363473 49.9480247262919,34.8517853643868 49.9473917574439,34.8526252891774 49.9476316507259,34.8536763180598 49.9478166278624),(34.8491128764165 49.948576760825,34.8495440677529 49.9485247368982,34.8511610352643 49.9490276325036,34.851349681474 49.9492877488597,34.8512239173342 49.949594106322,34.8497192392333 49.94925306676,34.8491128764165 49.948576760825)))";
        // await polygonGenerationService.GeneratePolygonAsync("", wktLine, new List<int>() { 14 });

        ThreadPool.GetMinThreads(out var workerThreads, out var ioThreads);
        Console.WriteLine($"workerThreads = {workerThreads}, ioThreads = {ioThreads}");
        
        ThreadPool.SetMinThreads(6, ioThreads);
        ThreadPool.SetMaxThreads(16, ioThreads * 2);
        
        CreateHostBuilder(args).Build().Run();

        // var c = 0;
        //
        // try
        // {
        //     // var filePath = "/Users/ruslandudchenko/Documents/tile_11_1235_692.mvt";
        //     // var filePath = "/Users/ruslandudchenko/Documents/11/tile_11_1235_692.mvt";
        //     var filePath = "/Users/ruslandudchenko/Documents/698.pbf";
        //     // var filePath = "/Users/ruslandudchenko/Documents/tile_12_2466_1397.mvt";
        //     var bytes = File.ReadAllBytes(filePath);
        //     
        //     var decoder = new VectorTileDecoder();
        //     // decoder.SetAutoScale(false);
        //     var features = decoder.Decode(bytes);
        //
        //     //var count = features.Count();
        //
        //     foreach (var feature in features)
        //     {
        //         Console.WriteLine(feature.GetLayerName() + " " + (c++));
        //     }
        //
        //     Console.WriteLine(bytes.Length);
        // }
        // catch (Exception e) // 496, d06abf06-760c-40d2-9636-61fecd9f8305
        // {
        // }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                var configurationBuilder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                var configuration = configurationBuilder.Build();

                services.AddSingleton(configuration);
                services.AddScoped<ITileGenerationService, TileGenerationService>();
                services.AddScoped<ISteadGenerationService, SteadGenerationService>();
                services.AddScoped<IPolygonGenerationService, PolygonGenerationService>();
                services.AddScoped<IFileGenerationService, FileGenerationService>();
                services.AddScoped<ITileDownloadingService, TileDownloadingService>();
                services.AddScoped<ICsvGenerationService, CsvGenerationService>();

                services.Configure<TileGenerationBoundariesOptions>(
                    configuration.GetSection(Constants.AppSettings.TILE_GENERATION_BOUNDARIES_OPTIONS));
                services.Configure<TileMvtFileOptions>(
                    configuration.GetSection(Constants.AppSettings.TILE_MVT_FILE_OPTIONS));

                services.DataAccessInitializer(configuration);

                var converterSqlServerConnection =
                    configuration.GetConnectionString(Constants.AppSettings.CONVERTER_SQL_SERVER_CONNECTION);

                services.AddDbContext<DataConverterDbContext>(options =>
                    options.UseNpgsql(converterSqlServerConnection,
                        x => x.MigrationsAssembly("FarmApp.DataConverter")));

                using (var context = services.BuildServiceProvider().GetService<DataConverterDbContext>())
                {
                    context?.Database.Migrate();
                }

                services.AddHostedService<WorkerService>();
            });
}


//
// services.AddSingleton(configuration);
// services.AddScoped<ITileGenerationService, TileGenerationService>();
// services.AddScoped<ISteadGenerationService, SteadGenerationService>();
// services.AddScoped<IPolygonGenerationService, PolygonGenerationService>();
// services.AddScoped<IFileGenerationService, FileGenerationService>();
//
// services.Configure<TileGenerationBoundariesOptions>(
//     configuration.GetSection(Constants.AppSettings.TileGenerationBoundariesOptions));
// services.Configure<TileMvtFileOptions>(configuration.GetSection(Constants.AppSettings.TileMvtFileOptions));
//
// services.DataAccessInitializer(configuration);
//
// var converterSqlServerConnection =
//     configuration.GetConnectionString(Constants.AppSettings.ConverterSqlServerConnection);
//
// services.AddDbContext<DataConverterDbContext>(options =>
//     options.UseNpgsql(converterSqlServerConnection, x => x.MigrationsAssembly("FarmApp.DataConverter")));
//
// await using (var context = services.BuildServiceProvider().GetService<DataConverterDbContext>())
// {
//     context?.Database.Migrate();
// }
//
// var serviceProvider = services.BuildServiceProvider();
//
// var tileGenerationService = serviceProvider.GetRequiredService<ITileGenerationService>();
// var steadGenerationService = serviceProvider.GetRequiredService<ISteadGenerationService>();
// var fileGenerationService = serviceProvider.GetRequiredService<IFileGenerationService>();
// var polygonGenerationService = serviceProvider.GetRequiredService<IPolygonGenerationService>();
//
// // Console.WriteLine("Tile generation started...");
// // await tileGenerationService.GenerateTilesAsync(14);
// // Console.WriteLine("Tile generation finished.");
//
// // Console.WriteLine("Stead generation started...");
// //await steadGenerationService.GenerateSteadsAsync(14, "/Users/ruslandudchenko/development/Місто Севастополь.csv");
//
// // await steadGenerationService.GenerateSteadsAsync(14, "/Users/ruslandudchenko/development/Харківська область.csv");
// // await steadGenerationService.GenerateSteadsAsync(14, "/Users/ruslandudchenko/development/Дніпропетровська область.csv");
// // Console.WriteLine("Stead generation finished.");
//
// Console.WriteLine("Mvt file generation started...");
// await fileGenerationService.GenerateMvtFilesAsync(14);
// Console.WriteLine("Mvt file generation finished.");

// await polygonGenerationService.GeneratePolygonAsync("", wkt, 14);

// return;
//
// // await ((FileGenerationService)fileGenerationService).GenerateTileFileAsync(new TileEntity()
// // {
// //     X = 9849,
// //     Y = 5557,
// //     Z = 14,
// //     Polygons = new List<PolygonEntity>()
// //     {
// //         new PolygonEntity()
// //         {
// //             SteadId = "",
// //             WktCoordinates = "MULTIPOLYGON(((36.4325777193104 49.9867616062635,36.432573227734 49.9867673821123,36.4325462782755 49.986764494188,36.4325462782755 49.9867587183388,36.4309697349518 49.9866027701488,36.4295908209907 49.9864410455657,36.4280726681606 49.9862244492902,36.4267790941514 49.9860251798554,36.426774602575 49.986033843761,36.4267476531165 49.9860309557926,36.4267521446929 49.9860222918865,36.4265365490247 49.9859876362462,36.4252654328977 49.9857450460654,36.424766867915 49.9855833185985,36.4235676170107 49.9851241251484,36.4235002433644 49.9850981329352,36.4234283781417 49.9851212371254,36.4232981224255 49.9851616694315,36.4231813414385 49.9851183491022,36.4222291272374 49.9847400165684,36.4222336188138 49.9845898379437,36.422053955757 49.9845147484555,36.420580718691 49.9839169164258,36.420526819774 49.9840439924549,36.4203516482935 49.9844540991679,36.4204190219399 49.9846504870601,36.4201944431188 49.9847631209306,36.4197946928174 49.9856728463706,36.419394942516 49.9865854425409,36.4189323101446 49.9877203876717,36.4184651861969 49.9888553060223,36.4181148432361 49.9894877298194,36.4176836518997 49.9905302002094,36.4172479689869 49.9915755356389,36.4168167776505 49.992617960774,36.4163855863142 49.9936603633132,36.4163766031613 49.9938509386196,36.417526446725 49.9932878730337,36.4186673071358 49.9927507889466,36.4198306254288 49.9922194740551,36.4210253847566 49.991639063579,36.4212769130362 49.9907121248592,36.4215958149621 49.989747627599,36.4218787842766 49.988783110996,36.4221842114732 49.9878185750503,36.4221976862024 49.9877752571138,36.4222156525081 49.9877232755386,36.4236215159277 49.9879023229501,36.4250408540767 49.9880842575404,36.4264557006491 49.9882661914425,36.427875038798 49.9884481246563,36.429294376947 49.9886271693695,36.4307137150959 49.988809101218,36.431836609201 49.988953491084,36.4321330532448 49.9889910323782,36.432177969009 49.9888437548253,36.4323216994544 49.9883412750744,36.4323082247252 49.9883326115844,36.4322678005374 49.9883037332731,36.4322543258081 49.988162229297,36.4326361098039 49.9867673821123,36.4325777193104 49.9867616062635),(36.4208322469706 49.9840613209783,36.4207603817478 49.9840324401025,36.4207738564771 49.9840151115687,36.4208457216998 49.9840439924549,36.4208322469706 49.9840613209783),(36.4213532698353 49.984266374698,36.4213308119532 49.984257710474,36.4213442866825 49.9842432700972,36.4213667445646 49.9842519343238,36.4213532698353 49.984266374698),(36.4217799695953 49.9844772036675,36.4217709864425 49.9844627633566,36.4218473432416 49.9844396588501,36.4218563263945 49.984456987231,36.4217799695953 49.9844772036675),(36.4221033630976 49.984838210031,36.4220988715212 49.9848179937463,36.4221842114732 49.9848151057049,36.4221842114732 49.9848353219909,36.4221033630976 49.984838210031),(36.422619894386 49.985034596355,36.4225974365038 49.9850259322694,36.4226109112331 49.9850086040935,36.4226333691152 49.9850201562114,36.422619894386 49.985034596355),(36.4231049846394 49.9852280938606,36.423069052028 49.9851789975523,36.4230960014865 49.9851703334927,36.4231319340979 49.9852194298098,36.4231049846394 49.9852280938606),(36.4237158390326 49.9852685260769,36.4236933811505 49.9852598620334,36.4237338053383 49.9852136537751,36.4237607547968 49.9852223178269,36.4237158390326 49.9852685260769),(36.4245198312119 49.9855544386365,36.4244928817533 49.9855457746445,36.424510848059 49.9855313346544,36.4245333059411 49.985539998649,36.4245198312119 49.9855544386365),(36.4251127192994 49.9858085817072,36.4250857698409 49.985799917761,36.4251217024522 49.9857508220363,36.4251441603343 49.9857565980065,36.4251127192994 49.9858085817072),(36.4259481525136 49.985921212866,36.4259212030551 49.9859154369155,36.4259256946315 49.9858981090601,36.42595264409 49.9859038850126,36.4259481525136 49.985921212866),(36.4325462782755 49.986880011026,36.4325148372405 49.9868771231084,36.4325328035462 49.9868251405626,36.4325597530047 49.9868280284832,36.4325462782755 49.986880011026),(36.4324294972885 49.9873131966976,36.43240254783 49.987310308806,36.4324070394064 49.987292981453,36.432433988865 49.9872958693456,36.4324294972885 49.9873131966976),(36.4323082247252 49.9878012478804,36.4322767836902 49.9877983600181,36.4322812752667 49.987781032841,36.4323127163016 49.9877839207043,36.4323082247252 49.9878012478804),(36.432177969009 49.9882950697763,36.4321510195505 49.9882921819437,36.4321555111269 49.9882719671106,36.4321824605854 49.9882748549444,36.432177969009 49.9882950697763),(36.4320566964456 49.9887802231927,36.4320297469871 49.9887773353893,36.4320342385635 49.9887600085647,36.4320611880221 49.9887628963693,36.4320566964456 49.9887802231927),(36.4202618167651 49.9856699583806,36.4200731705555 49.9856353024865,36.4203381735643 49.9850172681822,36.420526819774 49.9850490364942,36.4202618167651 49.9856699583806)))"
// //         }
// //     }
// // });
//
// var coords = new[,]
// {
//     { 30.3488542385979, 50.5039335292147 },
//     { 30.349015935349, 50.5039420994819 },
//     { 30.3490249185019, 50.5038992481304 },
//     { 30.3488587301743, 50.5038906778554 },
//     { 30.3488542385979, 50.5039335292147 }
// };
//
// var zoom = 16;
// // var lng = 30.3488542385979;
// // var lat = 50.5039335292147;
//
// // var xL = ConverterMath.long2tilex(lng, zoom);
// // var yL = ConverterMath.lat2tiley(lat, zoom);
//
// // Console.WriteLine($"{xL}, {yL}");
//
// // ConverterMath.ConvertToTileCoords(zoom, lng, lat, out var xT, out var yT);
//
// var decode = false;
//
// if (decode)
// {
//     try
//     {
//         // var filePath = "test-files/10_262_382.mvt";
//         // var filePath = "test-files/map_tile_16_38292_22082.mvt";
//         var filePath = "test-files/16_38292_22082.mvt";
//         var bytes = File.ReadAllBytes(filePath);
//
//         var decoder = new VectorTileDecoder();
//         // decoder.SetAutoScale(false);
//         var features = decoder.Decode(bytes);
//
//         //var count = features.Count();
//
//         var c = 0;
//
//         foreach (var feature in features)
//         {
//             Console.WriteLine(feature.GetLayerName() + " " + (c++));
//         }
//
//         Console.WriteLine(bytes.Length);
//     }
//     catch (Exception e)
//     {
//     }
// }
// else
// {
//     try
//     {
//         var encoder = new VectorTileEncoder();
//
//         encoder.AddFeature("marker-points-layer", new(), new Point(40, 40));
//         encoder.AddFeature("marker-points-layer", new(), new Point(50, 50));
//         encoder.AddFeature("marker-points-layer", new(), new Point(60, 60));
//
//         var length = coords.GetLength(0);
//
//         var exteriorRingCoordinates = new List<Coordinate>();
//
//         var tileX = 38292;
//         var tileY = 22082; // 14 14 13 14 16
//
//         for (var i = 0; i < length; i++)
//         {
//             var longitude = coords[i, 0];
//             var latitude = coords[i, 1];
//
//             ConverterMath.ConvertToTileCoords(zoom, longitude, latitude, out var coordTileX, out var coordTileY);
//
//             var dX = coordTileX - tileX;
//             var dY = coordTileY - tileY;
//
//             var x = (dX * 256);
//             var y = (dY * 256);
//
//             Console.WriteLine($"{dX}, {dY} | {x}, {y}");
//
//             exteriorRingCoordinates.Add(new Coordinate(x, y));
//         }
//
//         // exteriorRingCoordinates.Reverse();
//
//         // Coordinate[] exteriorRingCoordinates = new Coordinate[]
//         // {
//         //     new Coordinate(0, 0),
//         //     new Coordinate(256, 0),
//         //     new Coordinate(256, 256),
//         //     new Coordinate(0, 256), 
//         //     new Coordinate(0, 0) // Make sure to close the ring by repeating the first point
//         // };
//
//         // 222.625 183, 223.0625 186.1875, 215.3125 186.8125, 215.125 183.625, 222.625 183
//         // Coordinate[] exteriorRingCoordinates = new Coordinate[]
//         // {
//         //     new Coordinate(222.625, 183),
//         //     new Coordinate(223.0625, 186.1875),
//         //     new Coordinate(215.3125, 186.8125),
//         //     new Coordinate(215.125, 183.625), 
//         //     new Coordinate(222.625, 183) // Make sure to close the ring by repeating the first point
//         // };
//
//         LinearRing exteriorRing = new LinearRing(exteriorRingCoordinates.ToArray());
//         // LinearRing exteriorRing = new LinearRing(exteriorRingCoordinates);
//         Polygon polygon = new Polygon(exteriorRing);
//
//         encoder.AddFeature("marker-polygons-layer", new(),
//             polygon);
//
//         // Add one or more features with a layer name, a Map with attributes and a JTS Geometry. 
//         // The Geometry uses (0,0) in upper left and (256,256) in lower right.
//         var data = encoder.Encode();
//
//         Console.WriteLine(data.Length);
//
//         var filePath = "/Users/ruslandudchenko/Documents/map_tile_16_38292_22082.mvt";
//         File.WriteAllBytes(filePath, data);
//
//         Console.WriteLine("Byte array saved to file.");
//     }
//     catch (Exception e)
//     {
//     }
// }