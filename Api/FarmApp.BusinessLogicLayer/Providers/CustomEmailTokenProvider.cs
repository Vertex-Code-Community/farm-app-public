using Microsoft.AspNetCore.Identity;
using FarmApp.Entities.Entity;

namespace FarmApp.BusinessLogicLayer.Providers;

public class CustomEmailTokenProvider : IUserTwoFactorTokenProvider<UserEntity>
{
	private readonly Random _random = new();
	
	public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<UserEntity> manager, UserEntity user)
	{
		return Task.FromResult(true);
	}

	public async Task<string> GenerateAsync(string purpose, UserManager<UserEntity> manager, UserEntity user)
	{
		var verificationCodeNumber = _random.Next(100000, 999999);
		var verificationCode = verificationCodeNumber.ToString("D6");

		user.EmailVerificationCode = verificationCode;
		await manager.UpdateAsync(user);
		
		return verificationCode;
	}

	public Task<bool> ValidateAsync(string purpose, string token, UserManager<UserEntity> manager, UserEntity user)
	{
		return Task.FromResult(user.EmailVerificationCode == token);
	}
}


