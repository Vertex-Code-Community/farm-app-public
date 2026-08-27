
using FarmApp.DataAccessLayer.DbContext;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using FarmApp.Entities.Entity;
using FarmApp.Models.Options;
using FarmApp.Shared.Helpers;
using Microsoft.Extensions.Options;

namespace FarmApp.DataAccessLayer.Services;

public static class DataSeederService
{
    public static async Task SeedDataAsync(this IServiceCollection service)
    {
        var serviceProvider = service.BuildServiceProvider();
        var userRepository = service.BuildServiceProvider().GetRequiredService<IUserRepository>();
                
        var usersOptions = serviceProvider.GetRequiredService<IOptions<UsersOptions>>().Value;
        
        await SeedAdminUserAsync(userRepository,  usersOptions);
    }

    private static async Task SeedAdminUserAsync(IUserRepository repository,
        UsersOptions options)
    {
        if (options?.Users == null || options.Users.Count == 0)
            return;
        
        
        var usersSeeded = await repository.GetAsync(x => x.Email == options.Users[0].Email);

        // todo remove after this commit
        var testEmail = "farm-app-test@gmail.com";
        var testUserExists = await repository.GetAsync(x => x.Email == testEmail);
        if (!testUserExists.Any())
        {
            var passwordHash = SecurityHelper.HashPassword("Vertexcode1");

            var entity =  new UserEntity
            {
                Email = testEmail,
                PasswordHash = passwordHash.Hash,
                PasswordSalt = passwordHash.Salt,
                EmailConfirmed = true,
                EmailConfirmToken = Guid.NewGuid().ToString(),
                VerificationSentDate = DateTime.UtcNow
            };
            
            await repository.CreateAsync(entity);
            await repository.AddDefaultNotificationPreferencesAsync(entity.Id);
            return;
        }
        if (usersSeeded.Any()) return;
     
        var users = options.Users
            .Select(u =>
            {
                var passwordHash = SecurityHelper.HashPassword(u.Password);

                return new UserEntity
                {
                    Email = u.Email,
                    PasswordHash = passwordHash.Hash,
                    PasswordSalt = passwordHash.Salt,
                    EmailConfirmed = true,
                    EmailConfirmToken = Guid.NewGuid().ToString(),
                    VerificationSentDate = DateTime.UtcNow
                };
            })
            .ToList();

        await repository.AddRangeAsync(users);
        foreach (var u in users)
            await repository.AddDefaultNotificationPreferencesAsync(u.Id);
    }
}