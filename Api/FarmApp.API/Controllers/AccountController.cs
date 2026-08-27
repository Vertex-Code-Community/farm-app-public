using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.ViewModels.Accounts;
using FarmApp.ViewModels.Verifications;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp(SignUpRequestModel model)
    {
        var response = await _accountService.SignUpAsync(model);
        return Ok(response);
    }

    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn(SignInRequestModel requestModel)
    {
        var token = await _accountService.SignInAsync(requestModel);
        return Ok(token);
    }
    
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailRequestModel requestModel)
    {
        var result = await _accountService.ConfirmEmailAsync(requestModel);
        return Ok(result);
    }
    [HttpPost("resend-code")]
    public async Task<IActionResult> ResendCode(VerificationResendRequestModel model)
    {
        var result = await _accountService.ResendCodeAsync(model);
        return Ok(result);
    }
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestModel model)
    {
        var response = await _accountService.ForgotPasswordAsync(model);
        return Ok(response);
    }
    
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequestModel model)
    {
        var result = await _accountService.ResetPasswordAsync(model);
        return Ok(result);
    }
    [HttpPost("validate-reset-code")]
    public IActionResult ValidateResetCode(ResetCodeModel model)
    {
        var result = _accountService.ValidateResetCode(model);
        return Ok(result);
    }
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshTokenAsync(TokenModel model)
    {
        var accessToken = await _accountService.UpdateTokensAsync(model.RefreshToken);
        
        if (accessToken is null) return BadRequest(accessToken);
        return Ok(accessToken);
    }
}
