using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.Models.PushNotification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class AdminNotificationsController
    : ControllerBase
{
    private readonly INotificationService _notificationService;

    public AdminNotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("resume-delayed")]
    public async Task<IActionResult> ResumeDelayedNotificationAsync()
    {
        await _notificationService.ResumeDelayedNotification();
        return Ok();
    }

    [HttpPost("send")]
    public async Task<ActionResult<List<NotificationDeliveryOutcome[]>>> RequestSendNotificationAsync([FromBody] NotificationModel notificationModel)
    {
        var result = await _notificationService.RequestSendNotification(notificationModel);
        return Ok(result);
    }

    [HttpPut("{notificationId}/cancel")]
    public async Task<IActionResult> CancelAsync(long notificationId)
    {
        await _notificationService.CancelAsync(notificationId);
        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] NotificationModel notification)
    {
        await _notificationService.UpdateAsync(notification);
        return Ok();
    }
}
