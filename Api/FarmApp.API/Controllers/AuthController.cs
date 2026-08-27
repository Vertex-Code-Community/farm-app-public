using FarmApp.BusinessLogicLayer.Providers;
using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.Models;
using FarmApp.Models.NoLoyalty;
using FarmApp.Models.User;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly INoLoyaltyUserService _noLoyaltyUserService;

    public AuthController(INoLoyaltyUserService noLoyaltyUserService)
    {
        _noLoyaltyUserService = noLoyaltyUserService;
    }

    [HttpPost("login")]
    public async Task<UserTokenModel> LoginAsync([FromBody] CredentialsModel dto)
    {
        var modal = new NoLoyaltySignInModel()
        {
            Email = dto.Email,
            Password = dto.Password
        };
        var result = await _noLoyaltyUserService.SignInAsync(modal);

        if (result is null)
        {
            var apiResponse = new ApiResponse
            {
                Message = "Invalid email or password",
            };
            // return Unauthorized(apiResponse);
        }

        var tokenModel = new UserTokenModel(result.Data.AccessToken);
        return tokenModel;
    }
}
