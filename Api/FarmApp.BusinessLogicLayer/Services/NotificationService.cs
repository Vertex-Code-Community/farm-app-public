using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.BusinessLogicLayer.Utilities;
using FarmApp.Models.PushNotification;
using FarmApp.Shared.Constants;
using Microsoft.Extensions.Hosting;
using PushSharp.Net;

namespace FarmApp.BusinessLogicLayer.Services;

public class NotificationService : BackgroundService, INotificationService
{
    private readonly INotificationHistoryService _notificationHistoryService;
    private readonly IPushClient _pushClient;
    private readonly IDeviceRegistrationStore _deviceStore;
    private readonly List<NotificationTimer> _timers = new();

    public NotificationService(
        INotificationHistoryService notificationHistoryService,
        IPushClient pushClient,
        IDeviceRegistrationStore deviceStore)
    {
        _notificationHistoryService = notificationHistoryService;
        _pushClient = pushClient;
        _deviceStore = deviceStore;
    }

    public async Task ResumeDelayedNotification()
    {
        var notifications = await _notificationHistoryService.GetAllDelayedNotificationAsync();

        notifications.ForEach(x => { _ = RequestSendNotification(x, default); });
    }

    public async Task<List<NotificationDeliveryOutcome[]>?> RequestSendNotification(
        NotificationModel notificationModel,
        CancellationToken cancellationToken)
    {
        var notificationRequest = new NotificationRequest
        {
            Title = notificationModel.Title,
            Url = notificationModel.UrlForRedirection ?? string.Empty,
            Text = notificationModel.Message,
            TagExpression = NotificationTagExpressionBuilder.Build(notificationModel)
        };

        if (notificationModel.Id is 0)
        {
            ApplyNotificationPersistenceDefaults(notificationModel);
            notificationModel.Id = await _notificationHistoryService.AddToHistory(notificationModel);
        }

        if (notificationModel.TypeOfSend == TypeOfSend.Now ||
            (notificationModel.Status == NotificationStatus.Scheduled && notificationModel.DateTimeOfSend < DateTime.UtcNow))
        {
            return await SendPushNowAsync(notificationModel, notificationRequest, cancellationToken)
                .ConfigureAwait(false);
        }

        _timers.Add(new NotificationTimer
        {
            NotificationId = notificationModel.Id,
            Timer = new Timer(_ =>
            {
                _ = SendPushNowAsync(notificationModel, notificationRequest, CancellationToken.None);
            }, null, notificationModel.DateTimeOfSend - DateTime.UtcNow, Timeout.InfiniteTimeSpan)
        });

        return null;
    }

    private async Task<List<NotificationDeliveryOutcome[]>> SendPushNowAsync(
        NotificationModel notificationModel,
        NotificationRequest notificationRequest,
        CancellationToken cancellationToken)
    {
        var tokens = await PushSharpAudienceHelper
            .ResolveDeviceTokensAsync(_deviceStore, notificationModel, cancellationToken)
            .ConfigureAwait(false);

        if (tokens.Count == 0)
        {
            await _notificationHistoryService.CompeteAsync(notificationModel.Id).ConfigureAwait(false);
            return MapEmptyOutcome();
        }

        var push = new PushNotification
        {
            Title = notificationRequest.Title,
            Body = notificationRequest.Text,
            Data = string.IsNullOrWhiteSpace(notificationRequest.Url)
                ? null
                : new Dictionary<string, string> { ["url"] = notificationRequest.Url }
        };

        var batch = await _pushClient.SendBatchAsync(tokens, push, cancellationToken).ConfigureAwait(false);

        await _notificationHistoryService.CompeteAsync(notificationModel.Id).ConfigureAwait(false);

        return MapToLegacyShape(batch);
    }

    private static List<NotificationDeliveryOutcome[]> MapEmptyOutcome() =>
    [
        new[]
        {
            new NotificationDeliveryOutcome
            {
                Success = 0,
                Failure = 0,
                Results = []
            }
        }
    ];

    private static List<NotificationDeliveryOutcome[]> MapToLegacyShape(BatchPushResult batch)
    {
        var outcome = new NotificationDeliveryOutcome
        {
            Success = batch.SuccessCount,
            Failure = batch.FailureCount,
            Results = batch.Results.Select(tr => new NotificationDeliveryRegistrationResult
            {
                ApplicationPlatform = GuessPlatform(tr.DeviceToken),
                PnsHandle = tr.DeviceToken,
                RegistrationId = tr.DeviceToken.Length > 8 ? tr.DeviceToken[..8] : tr.DeviceToken,
                Outcome = tr.Result.IsSuccess ? "Success" : (tr.Result.ErrorCode ?? "Failed")
            }).ToList()
        };

        return new List<NotificationDeliveryOutcome[]> { new[] { outcome } };
    }

    private static string GuessPlatform(string token) =>
        token.Length == 64 && token.All(Uri.IsHexDigit) ? "apns" : "fcm";

    private static void ApplyNotificationPersistenceDefaults(NotificationModel m)
    {
        if (string.IsNullOrEmpty(m.TypeUrlForRedirection))
            m.TypeUrlForRedirection = TypeOfRedirection.None;
        if (string.IsNullOrEmpty(m.Sender))
            m.Sender = "FarmApp";
        if (string.IsNullOrEmpty(m.TypeOfSend))
            m.TypeOfSend = TypeOfSend.Now;
        if (string.IsNullOrEmpty(m.Platform))
            m.Platform = Platform.All;
        if (string.IsNullOrEmpty(m.TypeOfTargetUser))
            m.TypeOfTargetUser = TargetUserType.SpecificUser;
        if (string.IsNullOrEmpty(m.Status))
            m.Status = NotificationStatus.Sent;
        if (string.IsNullOrEmpty(m.Title))
            m.Title = string.Empty;
        if (string.IsNullOrEmpty(m.Message))
            m.Message = string.Empty;
        if (m.DateTimeOfSend == default)
            m.DateTimeOfSend = DateTime.UtcNow;
    }

    public async Task CancelAsync(long notificationId)
    {
        var timer = _timers.FirstOrDefault(x => x.NotificationId == notificationId);

        if (timer is not null)
        {
            await timer.Timer.DisposeAsync();
            _timers.Remove(timer);
        }

        await _notificationHistoryService.CancelAsync(notificationId);
    }

    public async Task UpdateAsync(NotificationModel notification)
    {
        var timer = _timers.FirstOrDefault(x => x.NotificationId == notification.Id);

        if (timer is not null)
        {
            await timer.Timer.DisposeAsync();
            _timers.Remove(timer);

            await _notificationHistoryService.UpdateAsync(notification);
            await RequestSendNotification(notification, default);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ResumeDelayedNotification();
    }
}
