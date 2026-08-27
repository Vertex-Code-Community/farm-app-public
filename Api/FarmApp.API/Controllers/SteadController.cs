using Microsoft.AspNetCore.Mvc;
using FarmApp.BusinessLogicLayer.Services.Interfaces;

namespace FarmApp.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SteadController : ControllerBase
{
    private readonly ISteadService _steadService;

    public SteadController(ISteadService steadService)
    {
        _steadService = steadService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(string id)
    {
        var result = await _steadService.GetByIdAsync(id);
        return Ok(result);
    }
}
