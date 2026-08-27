using FarmApp.BusinessLogicLayer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmApp.Shared.Constants;
using FarmApp.ViewModels.Users;

namespace FarmApp.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = Constants.JwtDetails.BEARER)]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("filter-with-pagination")]
    public async Task<IActionResult> FilterWithPaginationAsync(int skip, int take)
    {
        var result = await _userService.FilterByAsync(skip, take);
        return Ok(result);
    }
    
    [HttpGet("get-current-user")]
    public async Task<IActionResult> GetCurrentUserAsync()
    {
        var result = await _userService.GetCurrentUserAsync();
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUserAsync(UpdateUserModel model)
    {
        await _userService.UpdateAsync(model);
        return Ok();
    }

    [HttpPut("selected-location")]
    public async Task<IActionResult> UpdateSelectedLocationAsync([FromBody] UpdateSelectedLocationModel model)
    {
        await _userService.UpdateSelectedLocationAsync(model);
        return Ok();
    }

    [HttpPut("notification-preferences")]
    public async Task<IActionResult> UpdateNotificationPreferencesAsync([FromBody] UpdateNotificationPreferencesModel model)
    {
        await _userService.UpdateNotificationPreferencesAsync(model);
        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteByIdAsync([FromQuery] string id)
    {
        await _userService.DeleteAsync(id);
        return Ok();
    }
}
