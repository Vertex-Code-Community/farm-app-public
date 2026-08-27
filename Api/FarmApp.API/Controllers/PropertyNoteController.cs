using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FarmApp.Api.Attributes;
using FarmApp.Api.ModelBinders;
using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.ViewModels.PropertyNotes;

namespace FarmApp.Api.Controllers;

[Route("api/property-note")]
[ApiController]
[Authorize]
public class PropertyNoteController : ControllerBase
{
    private readonly IPropertyNoteService _propertyNoteService;

    public PropertyNoteController(IPropertyNoteService propertyNoteService)
    {
        _propertyNoteService = propertyNoteService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(string id)
    {
        var result = await _propertyNoteService.GetByIdAsync(id);
        return Ok(result);
    }
    
    [HttpGet("{propertyId}/all/{day}")]
    public async Task<IActionResult> GetAllByDateAsync(
        string propertyId, 
        [ModelBinder(BinderType = typeof(DateTimeModelBinder))] DateTime day)
    {
        var propertyNotes = await _propertyNoteService.GetAllByDateAsync(propertyId, day);
        return Ok(propertyNotes);
    }
    
    [HttpPost]
    [SubscriptionRequired]
    public async Task<IActionResult> Create(CreatePropertyNoteModel model)
    {
        var propertyNoteModel = await _propertyNoteService.CreateAsync(model);
        return Ok(propertyNoteModel);
    }
    
    [HttpPatch("{id}")]
    [SubscriptionRequired]
    public async Task<IActionResult> Update(string id, UpdatePropertyNoteModel model)
    {
        var propertyNoteModel = await _propertyNoteService.UpdateAsync(id, model);
        return Ok(propertyNoteModel);
    }
    
    [HttpDelete("{id}")]
    [SubscriptionRequired]
    public async Task<IActionResult> Delete(string id)
    {
        await _propertyNoteService.DeleteAsync(id);
        return Ok();
    }
}