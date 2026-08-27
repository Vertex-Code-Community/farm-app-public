using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.ViewModels.Weather;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        public WeatherController(IWeatherService weatherService)
        {
            _weatherService = weatherService;
        }
        [HttpGet]
        public async Task<IActionResult> GetWeather([FromQuery] WeatherRequestModel requestModel)
        {
            var result = await _weatherService.GetWeather(requestModel);
            return Ok(result);
        }
    }
}
