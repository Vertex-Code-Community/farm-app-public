using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.Models.PushNotification;
using FarmApp.Models.PushNotificationHistory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Controllers;

[ApiController]
[Route("api/notification-history")]
[Authorize]
public class NotificationHistoryController : ControllerBase
{
    private readonly INotificationHistoryService _notificationHistoryService;

    public NotificationHistoryController(INotificationHistoryService notificationHistoryService)
    {
        _notificationHistoryService = notificationHistoryService;
    }

    [HttpGet]
    public async Task<ActionResult<List<NotificationModel>>> GetAllAsync()
    {
        var notifications = await _notificationHistoryService.GetAllAsync();
        return Ok(notifications);
    }

    [HttpGet("my")]
    public async Task<ActionResult<List<NotificationModel>>> GetMyAsync(
        [FromQuery] string? appVersion = null,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationHistoryService.GetMyAsync(appVersion, cancellationToken);
        return Ok(notifications);
    }

    [HttpGet("delayed")]
    public async Task<ActionResult<List<NotificationModel>>> GetAllDelayedNotificationAsync()
    {
        var notifications = await _notificationHistoryService.GetAllDelayedNotificationAsync();
        return Ok(notifications);
    }

    [HttpPost]
    public async Task<ActionResult<NotificationIdResponse>> AddToHistoryAsync([FromBody] NotificationModel notificationModel)
    {
        var notificationId = await _notificationHistoryService.AddToHistory(notificationModel);
        var dto = new NotificationIdResponse
        {
            Id = notificationId
        };

        return Ok(dto);
    }

    [HttpPut("{notificationId}/complete")]
    public async Task<IActionResult> CompleteAsync(long notificationId)
    {
        await _notificationHistoryService.CompeteAsync(notificationId);
        return Ok();
    }

    [HttpPut("{notificationId}/cancel")]
    public async Task<IActionResult> CancelAsync(long notificationId)
    {
        await _notificationHistoryService.CancelAsync(notificationId);
        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] NotificationModel notification)
    {
        await _notificationHistoryService.UpdateAsync(notification);
        return Ok();
    }
}
