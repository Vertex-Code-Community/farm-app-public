using System.ComponentModel.DataAnnotations;
using System.Net;
using FarmApp.BusinessLogicLayer.Utilities;
using FarmApp.Models.PushNotification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PushSharp.Net;

namespace FarmApp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IDeviceRegistrationStore _deviceStore;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        IDeviceRegistrationStore deviceStore,
        ILogger<NotificationsController> logger)
    {
        _deviceStore = deviceStore;
        _logger = logger;
    }

    [HttpPut]
    [Route("installations")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.UnprocessableEntity)]
    public async Task<IActionResult> UpdateInstallation(
        [Required] DeviceInstallation deviceInstallation)
    {
        if (!TryMapToRegistration(deviceInstallation, out var registration, out var error))
        {
            if (error is not null)
                _logger.LogWarning("Invalid device installation: {Error}", error);
            return new UnprocessableEntityResult();
        }

        try
        {
            await _deviceStore.SaveAsync(registration, HttpContext.RequestAborted).ConfigureAwait(false);
            return new OkResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Save device registration failed");
            return new UnprocessableEntityResult();
        }
    }

    [HttpDelete]
    [Route("installations/{installationId}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.UnprocessableEntity)]
    public async Task<ActionResult> DeleteInstallation([Required][FromRoute] string installationId)
    {
        if (string.IsNullOrWhiteSpace(installationId))
            return new UnprocessableEntityResult();

        try
        {
            await _deviceStore.RemoveAsync(installationId.Trim(), CancellationToken.None).ConfigureAwait(false);
            return new OkResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete device registration failed for {InstallationId}", installationId);
            return new UnprocessableEntityResult();
        }
    }

    private static bool TryMapToRegistration(
        DeviceInstallation installation,
        out DeviceRegistration registration,
        out string? error)
    {
        registration = null!;
        error = null;

        if (string.IsNullOrWhiteSpace(installation.InstallationId)
            || string.IsNullOrWhiteSpace(installation.Platform)
            || string.IsNullOrWhiteSpace(installation.PushChannel))
        {
            error = "Missing required fields";
            return false;
        }

        if (!TryNormalizePushPlatform(installation.Platform, out var platformKey))
        {
            error = $"Unsupported platform: {installation.Platform}";
            return false;
        }

        var tags = (installation.Tags ?? Array.Empty<string>())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        string? userId = null;
        var prefix = NotificationTagExpressionBuilder.UserShopperIdTagPrefix;
        foreach (var tag in tags)
        {
            if (!tag.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                continue;
            userId = tag[prefix.Length..].Trim();
            if (userId.Length > 0)
                break;
            userId = null;
        }

        registration = new DeviceRegistration
        {
            DeviceId = installation.InstallationId.Trim(),
            DeviceToken = installation.PushChannel.Trim(),
            Platform = platformKey,
            UserId = userId,
            Tags = tags
        };

        return true;
    }

    private static bool TryNormalizePushPlatform(string raw, out string platformKey)
    {
        switch (raw.Trim().ToLowerInvariant())
        {
            case "apns":
            case "apple":
                platformKey = "apns";
                return true;
            case "fcm":
            case "fcmv1":
            case "android":
                platformKey = "fcm";
                return true;
            default:
                platformKey = string.Empty;
                return false;
        }
    }
}
