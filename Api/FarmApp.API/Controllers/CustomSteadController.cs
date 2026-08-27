using FarmApp.Api.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FarmApp.Shared.Constants;
using FarmApp.ViewModels.CustomSteads;
using FarmApp.BusinessLogicLayer.Services.Interfaces;

namespace FarmApp.Api.Controllers;

[Route("api/custom-stead")]
[ApiController]
[Authorize]
public class CustomSteadController : ControllerBase
{
    private readonly ICustomSteadService _customSteadService;

    public CustomSteadController(ICustomSteadService customSteadService)
    {
        _customSteadService = customSteadService;
    }
    
    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = Constants.JwtDetails.BEARER)]
    public async Task<IActionResult> GetByIdAsync(string id)
    {
        var result = await _customSteadService.GetByIdAsync(id);
        return Ok(result);
    }
    
    [HttpGet]
    [Authorize(AuthenticationSchemes = Constants.JwtDetails.BEARER)]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _customSteadService.GetAllOfCurrentUserAsync();
        return Ok(result);
    }
    
    [HttpPost]
    [Authorize(AuthenticationSchemes = Constants.JwtDetails.BEARER)]
    [SubscriptionRequired]
    public async Task<IActionResult> Create(CreateCustomSteadModel model)
    {
        var customSteadModel = await _customSteadService.CreateAsync(model);
        return Ok(customSteadModel);
    }
    
    [HttpPatch("{id}")]
    [SubscriptionRequired]
    public async Task<IActionResult> Update(string id, UpdateCustomSteadModel model)
    {
        var customSteadModel = await  _customSteadService.UpdateAsync(id, model);
        return Ok(customSteadModel);
    }
    
    [HttpDelete("{id}")]
    [SubscriptionRequired]
    public async Task<IActionResult> Delete(string id)
    {
        await _customSteadService.DeleteAsync(id);
        return Ok();
    }
}