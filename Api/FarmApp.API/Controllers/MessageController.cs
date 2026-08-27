using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.ViewModels.Email;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MessageController : ControllerBase
{
    private readonly IEmailService _emailService;

    public MessageController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("contact-us")]
    public async Task<IActionResult> ContactUs(ContactUsModel model)
    {
        var result = await _emailService.ContactUsAsync(model);
        return Ok(result);
    }
}
