using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.Entities.Entity;
using FarmApp.Models.PushNotification;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Enums.PushNotification;
using FarmApp.ViewModels.Weather;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PushSharp.Net;

namespace FarmApp.BusinessLogicLayer.Workers;

public sealed class WeatherBroadcastWorker : BackgroundService
{
    private static readonly bool Enabled = true;
    private static readonly int PollIntervalSeconds = 60 * 60;
    private static readonly int SendAtUtcHour = 19;
    private static readonly string NotificationTitle = "Weather";
    private static readonly int UserBatchSize = 100;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WeatherBroadcastWorker> _logger;
    private readonly IPushClient _pushClient;
    private readonly IDeviceRegistrationStore _deviceStore;
    private DateTime? _lastDigestUtcDate;

    public WeatherBroadcastWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<WeatherBroadcastWorker> logger,
        IPushClient pushClient,
        IDeviceRegistrationStore deviceStore)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _pushClient = pushClient;
        _deviceStore = deviceStore;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WeatherBroadcastWorker started (Enabled={Enabled})", Enabled);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!Enabled)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Max(10, PollIntervalSeconds)), stoppingToken);
                    continue;
                }

                var now = DateTime.UtcNow;
                var hour = Math.Clamp(SendAtUtcHour, 0, 23);

            
                    var allSucceeded = await SendDailyDigestAsync(stoppingToken);
                    if (allSucceeded)
                    {
                        _lastDigestUtcDate = now.Date;
                        _logger.LogInformation("Weather broadcast fully completed for {Date}, skipping until tomorrow", now.Date);
                    }
                    else
                    {
                        _logger.LogWarning("Weather broadcast had failures for {Date}, will retry on next poll", now.Date);
                    }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WeatherBroadcastWorker loop error");
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(10, PollIntervalSeconds)), stoppingToken);
        }
    }

    private async Task<bool> SendDailyDigestAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var usersRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var skip = 0;
        var anyUser = false;
        var allSucceeded = true;

        while (!ct.IsCancellationRequested)
        {
            var batch = await usersRepository.GetUsersWithSavedLocationBatchAsync(skip, UserBatchSize, ct);
            if (batch.Count == 0)
                break;

            anyUser = true;
            _logger.LogInformation("Weather broadcast batch: {Count} users (skip {Skip})", batch.Count, skip);

            var tasks = batch.Select(async user =>
            {
                try { return await SendWeatherBroadcastForUserAsync(user, ct); }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Weather broadcast failed for user {UserId}", user.Id);
                    return false;
                }
            }).ToList();
            var results = await Task.WhenAll(tasks);

            if (results.Any(r => !r))
                allSucceeded = false;

            skip += batch.Count;
            if (batch.Count < UserBatchSize)
                break;
        }

        if (!anyUser)
            _logger.LogInformation("Weather broadcast: no users with saved location");

        return anyUser && allSucceeded;
    }

    private async Task<bool> SendWeatherBroadcastForUserAsync(UserEntity user, CancellationToken ct)
    {
        // Must match device registration tags (e.g. PushDeviceTags.WeatherAlerts / NotificationDisable)
        if (!UserAcceptsWeatherPush(user))
        {
            _logger.LogDebug("Skip weather push for user {UserId} (opts out of weather or all notifications).", user.Id);
            return true;
        }

        using var scope = _scopeFactory.CreateScope();
        var weather = scope.ServiceProvider.GetRequiredService<IWeatherService>();
        var history = scope.ServiceProvider.GetRequiredService<INotificationHistoryService>();

        var forecast = await weather.GetWeather(new WeatherRequestModel
        {
            Latitude = user.SelectedLocationLatitude!.Value,
            Longitude = user.SelectedLocationLongitude!.Value
        });

        if (forecast?.WeatherModel is null)
        {
            _logger.LogWarning("No weather data for user {UserId}", user.Id);
            return false;
        }

        var weatherModel = forecast.WeatherModel;
        var place = $"{weatherModel.City}, {weatherModel.Country}".Trim(' ', ',');
        var message = $"{place}: {Math.Round(weatherModel.Temperature)}°C, {weatherModel.Description}";

        var model = new NotificationModel
        {
            Title = NotificationTitle,
            Message = message,
            Sender = "FarmApp",
            TypeUrlForRedirection = TypeOfRedirection.None,
            TypeOfSend = TypeOfSend.Now,
            DateTimeOfSend = DateTime.UtcNow,
            Platform = Platform.All,
            TypeOfTargetUser = TargetUserType.SpecificUser,
            Status = NotificationStatus.Sent,
            NotificationKind = NotificationKind.Weather
        };

        model.Tags.Add(NotificationTagsType.User, new HashSet<string> { user.Id });

        model.Id = await history.AddToHistory(model);

        var handles = await _deviceStore.GetTokensByUserIdAsync(user.Id, ct);
        if (handles.Count == 0)
        {
            _logger.LogDebug("No push registrations for user {UserId}; completing history without PushSharp send.", user.Id);
            await history.CompeteAsync(model.Id);
            return true;
        }

        var push = new PushNotification
        {
            Title = NotificationTitle,
            Body = message,
            Data = new Dictionary<string, string>
            {
                ["kind"] = nameof(NotificationKind.Weather),
                ["userId"] = user.Id
            }
        };

        var batch = await _pushClient.SendBatchAsync(handles, push, ct);

        if (batch.FailureCount > 0)
        {
            _logger.LogWarning(
                "PushSharp weather digest for user {UserId}: success={Ok}, failed={Fail}",
                user.Id,
                batch.SuccessCount,
                batch.FailureCount);
            return false;
        }

        await history.CompeteAsync(model.Id);
        _logger.LogInformation("Weather broadcast sent to user {UserId} ({Count} device(s)) via PushSharp.Net", user.Id,
            handles.Count);
        return true;
    }

    /// <summary>Same targeting rules as <see cref="PushDeviceTags"/> on the device.</summary>
    private static bool UserAcceptsWeatherPush(UserEntity user)
    {
        var p = user.NotificationPreferences;
        if (p is null) return true;
        if (p.NotificationsDisabled) return false;
        return p.WeatherAlertsEnabled;
    }
}
