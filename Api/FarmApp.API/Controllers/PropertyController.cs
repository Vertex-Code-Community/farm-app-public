using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.ViewModels.Properties;
using FarmApp.Api.Attributes;

namespace FarmApp.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PropertyController : ControllerBase
{
    private readonly IPropertyService _propertyService;

    public PropertyController(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [HttpPost]
    [SubscriptionRequired]
    public async Task<IActionResult> CreateAsync(CreatePropertyModel model)
    {
        var propertyModel = await _propertyService.CreateAsync(model);
        return Ok(propertyModel);
    }

    [HttpDelete("{id}")]
    [SubscriptionRequired]
    public async Task<IActionResult> Delete(string id)
    {
        await _propertyService.DeleteAsync(id);
        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(string id)
    {
        var result = await _propertyService.GetByIdAsync(id);
        return Ok(result);
    }
    
    [HttpGet("{id}/preview")]
    public async Task<IActionResult> GetPreviewByIdAsync(string id)
    {
        var result = await _propertyService.GetPreviewByIdAsync(id);
        return Ok(result);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _propertyService.GetAllOfUserAsync();
        return Ok(result);
    }
    
    [HttpPatch("{id}")]
    [SubscriptionRequired]
    public async Task<IActionResult> UpdateAsync(string id, UpdatePropertyModel model)
    {
        var result = await _propertyService.UpdateAsync(id, model);
        return Ok(result);
    }
}
