using Microsoft.AspNetCore.Mvc;
using PushSharp.Net;

namespace FarmApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PushTestController : ControllerBase
    {
        private readonly IPushClient _pushClient;

        public PushTestController(IPushClient pushClient)
        {
            _pushClient = pushClient;
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromBody] PushTestRequest request, CancellationToken ct)
        {
            var notification = new PushNotification
            {
                Title = request.Title ?? "Test",
                Body = request.Body ?? "Hello from PushSharp.Net"
            };

            var result = await _pushClient.SendAsync(request.DeviceToken, notification, ct);

            return Ok(new
            {
                result.IsSuccess,
                result.IsDeadToken,
                result.ErrorCode,
                result.ErrorMessage
            });
        }
    }

    public sealed class PushTestRequest
    {
        public required string DeviceToken { get; init; }
        public string? Title { get; init; }
        public string? Body { get; init; }
    }
}
