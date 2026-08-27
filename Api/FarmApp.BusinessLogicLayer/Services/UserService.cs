using System.Net;
using System.Security;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.Shared.Exceptions;
using FarmApp.ViewModels.Users;
using FarmApp.Entities.Entity;
using FarmApp.Shared.Helpers;
using Org.BouncyCastle.Crypto.Generators;
using static FarmApp.Shared.Constants.Constants;
using ClaimTypes = System.Security.Claims.ClaimTypes;

namespace FarmApp.BusinessLogicLayer.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public UserService(
        IUserRepository userRepository, 
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<UserEntity> CreateAsync(CreateUserModel model)
    {
        var existingUser = await _userRepository.GetByEmailAsync(model.Email);
        if (existingUser is { EmailConfirmed: false }) return existingUser;
        
        if (existingUser is not null) throw new ServerException(ErrorMessages.USER_ALREADY_EXISTS, HttpStatusCode.BadRequest);

        var userEntity = _mapper.Map<UserEntity>(model);
        
        userEntity.Email = model.Email;
        userEntity.EmailConfirmed = false;
        userEntity.IconNumber = Math.Abs(model.Email.GetHashCode()) % 9 + 1;
        userEntity.EmailConfirmToken = string.Empty;
        userEntity.PasswordHash = model.PasswordHash;
        userEntity.PasswordSalt = model.Salt;
        userEntity.Created = DateTime.UtcNow;
        
        await _userRepository.CreateAsync(userEntity);
        await _userRepository.AddDefaultNotificationPreferencesAsync(userEntity.Id);
        
        return userEntity;
    }

    public async Task<List<UserViewModel>> FilterByAsync(int skip, int take)
    {
        var users = await _userRepository.FilterWithPagination(skip, take);
        return _mapper.Map<List<UserViewModel>>(users);
    }

    public async Task<UserModel> GetUserByIdAsync(string id)
    {
        var user = await _userRepository.GetByIdWithNotificationPreferencesAsync(id);
        if (user is null) throw new NullReferenceException(); 
        
        var userModel = _mapper.Map<UserModel>(user);
        return userModel;
    }

    public async Task<UserModel?> GetCurrentUserAsync()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return null;
        
        var userEntity = await _userRepository.GetByIdWithNotificationPreferencesAsync(userId);
        return userEntity is null ? null : _mapper.Map<UserModel>(userEntity);
    }

    public string? GetCurrentUserId()
    {
        if (_httpContextAccessor.HttpContext is null) throw new ServerException(ErrorMessages.USER_DOES_NOT_EXIST, HttpStatusCode.BadRequest);
        
        var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim?.Value;
    }

    public async Task UpdateAsync(UpdateUserModel model)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
            throw new ServerException(ErrorMessages.USER_DOES_NOT_EXIST, HttpStatusCode.Unauthorized);

        var isAdmin = _httpContextAccessor.HttpContext?.User.IsInRole(UserRole.Admin.ToString()) == true;
        var targetUserId = isAdmin ? model.Id : currentUserId;

        if (isAdmin && string.IsNullOrWhiteSpace(model.Id))
            throw new ServerException(ErrorMessages.USER_DOES_NOT_EXIST, HttpStatusCode.BadRequest);

        if (!isAdmin && !string.Equals(model.Id, currentUserId, StringComparison.Ordinal))
            throw new ServerException("Немає доступу змінювати цей профіль", HttpStatusCode.Forbidden);

        var entity = await _userRepository.GetByIdAsync(targetUserId);
        if (entity is null) throw new ServerException(ErrorMessages.USER_DOES_NOT_EXIST, HttpStatusCode.BadRequest);

        entity.FirstName = model.FirstName;
        entity.LastName = model.LastName;

        if (!string.IsNullOrEmpty(model.Password))
        {
            var hashedPassword = SecurityHelper.HashPassword(model.Password);

            entity.PasswordHash = hashedPassword.Hash;
            entity.PasswordSalt = hashedPassword.Salt;
        }

        await _userRepository.UpdateAsync(entity);
    }

    public async Task UpdateSelectedLocationAsync(UpdateSelectedLocationModel model)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
            throw new ServerException(ErrorMessages.USER_DOES_NOT_EXIST, HttpStatusCode.Unauthorized);

        var entity = await _userRepository.GetByIdAsync(currentUserId);
        if (entity is null) throw new ServerException(ErrorMessages.USER_DOES_NOT_EXIST, HttpStatusCode.BadRequest);

        entity.SelectedLocationLatitude = model.Latitude;
        entity.SelectedLocationLongitude = model.Longitude;

        await _userRepository.UpdateAsync(entity);
    }

    public async Task UpdateNotificationPreferencesAsync(UpdateNotificationPreferencesModel model)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
            throw new ServerException(ErrorMessages.USER_DOES_NOT_EXIST, HttpStatusCode.Unauthorized);

        var entity = await _userRepository.GetByIdForUpdateWithNotificationPreferencesAsync(currentUserId);
        if (entity is null) throw new ServerException(ErrorMessages.USER_DOES_NOT_EXIST, HttpStatusCode.BadRequest);

        _userRepository.AddNotificationPreferencesForUserIfMissing(entity);
        var p = entity.NotificationPreferences!;
        p.NotificationsDisabled = model.NotificationsDisabled;
        if (model.SystemNotificationsEnabled is { } s) p.SystemNotificationsEnabled = s;
        if (model.WeatherAlertsEnabled is { } w) p.WeatherAlertsEnabled = w;
        if (model.ActivityRemindersEnabled is { } a) p.ActivityRemindersEnabled = a;
        if (model.InAppNotificationsOnly is { } i) p.InAppNotificationsOnly = i;
        await _userRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var entity = await _userRepository.GetByIdAsync(id);
        if(entity is null) throw new ServerException(ErrorMessages.USER_DOES_NOT_EXIST, HttpStatusCode.BadRequest);
        
        await _userRepository.DeleteAsync(entity);
    }
}