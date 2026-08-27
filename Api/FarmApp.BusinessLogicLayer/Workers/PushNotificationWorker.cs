using FarmApp.DataAccessLayer.DbContext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using FarmApp.Models.PushNotification;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Enums.PushNotification;
using FarmApp.BusinessLogicLayer.Services.Interfaces;

namespace FarmApp.BusinessLogicLayer.Workers;

public class PushNotificationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PushNotificationWorker> _logger;

    public PushNotificationWorker(IServiceScopeFactory scopeFactory,
        ILogger<PushNotificationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PushNotificationWorker started");

        while(!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processed = await ProcessBatch(stoppingToken);

                if (!processed)
                {
                    await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PushNotification error");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }

    private async Task<bool> ProcessBatch(CancellationToken token)
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<FarmAppDbContext>();
        var pushService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var propertyNoteService = scope.ServiceProvider.GetRequiredService<IPropertyNoteService>();

        var now = DateTime.Now;

        var notifications = await db.PushNotificationsQueue
            .Where(x => x.Status == PushNotificationStatus.Pending && x.SendAt <= now)
            .OrderBy(x => x.SendAt)
            .Take(100).ToListAsync(token);

        if (!notifications.Any())
            return false;

        foreach (var notification in notifications)
        {
            notification.Status = PushNotificationStatus.Processing;
        }
        await db.SaveChangesAsync(token);

        foreach (var notification in notifications)
        {
            try
            {
                var propertyNote = await propertyNoteService.GetByIdAsync(notification.PropertyNoteId!);
                var model = new NotificationModel
                {
                    Title = "Field reminder",
                    Message = $"The event on {propertyNote!.Header} is coming",
                    Platform = Platform.All,
                    NotificationKind = NotificationKind.FieldReminder
                };
                model.Tags.Add(NotificationTagsType.User, new HashSet<string> { notification.UserId });

                await pushService.RequestSendNotification(model, token);

                notification.Status = PushNotificationStatus.Sent;
                notification.SentAt = DateTime.Now;
            }
            catch (Exception ex) 
            {
                notification.Status = PushNotificationStatus.Failed;
                notification.RetryCount++;
                notification.Error = ex.Message;

                _logger.LogError(ex, $"Failed to push {notification.Id}");
            }
        }
        await db.SaveChangesAsync(token);

        return true;
    }
}