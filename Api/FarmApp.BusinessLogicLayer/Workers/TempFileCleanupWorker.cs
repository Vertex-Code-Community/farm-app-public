using FarmApp.BusinessLogicLayer.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FarmApp.BusinessLogicLayer.Workers
{
    public class TempFileCleanupWorker : BackgroundService
    {
        private readonly ILogger<TempFileCleanupWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        public TempFileCleanupWorker(ILogger<TempFileCleanupWorker> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var fileStorageService = scope.ServiceProvider.GetRequiredService<IFileStorageService>();

                    await fileStorageService.CleanTempFiles();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Temp cleanup failed");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
