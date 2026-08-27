using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.ViewModels.PropertyNoteStatuses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/property-note-status")]
    public class PropertyNoteStatusController : ControllerBase
    {
        private readonly IPropertyNoteStatusService _statusService;
        public PropertyNoteStatusController(IPropertyNoteStatusService statusService)
        {
            _statusService = statusService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStatuses()
        {
            var result = await _statusService.GetPropertyNoteStatusesAsync();
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStatusByIdAsync(int id)
        {
            var result = await _statusService.GetStatusByIdAsync(id);
            return Ok(result);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStatus(int id)
        {
            await _statusService.DeleteAsync(id);
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> CreateStatus(CreatePropertyNoteStatusModel model)
        {
            var result = await _statusService.CreateAsync(model);
            return Ok(result);
        }
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateStatus(int id,UpdatePropertyNoteStatusModel model)
        {
            var result = await _statusService.UpdateAsync(id,model);
            return Ok(result);
        }

    }
}
