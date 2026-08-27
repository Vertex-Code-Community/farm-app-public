using AutoMapper;
using FarmApp.Entities.Entity;
using FarmApp.ViewModels.Users;

namespace FarmApp.BusinessLogicLayer.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserEntity, CreateUserModel>();
        CreateMap<CreateUserModel, UserEntity>();

        CreateMap<UserEntity, UserModel>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => DisplayName(s.FirstName, s.LastName, s.Email)))
            .ForMember(d => d.NotificationsDisabled,
                o => o.MapFrom(s => s.NotificationPreferences != null && s.NotificationPreferences.NotificationsDisabled))
            .ForMember(d => d.SystemNotificationsEnabled,
                o => o.MapFrom(s => s.NotificationPreferences == null || s.NotificationPreferences.SystemNotificationsEnabled))
            .ForMember(d => d.WeatherAlertsEnabled,
                o => o.MapFrom(s => s.NotificationPreferences == null || s.NotificationPreferences.WeatherAlertsEnabled))
            .ForMember(d => d.ActivityRemindersEnabled,
                o => o.MapFrom(s => s.NotificationPreferences == null || s.NotificationPreferences.ActivityRemindersEnabled))
            .ForMember(d => d.InAppNotificationsOnly,
                o => o.MapFrom(s => s.NotificationPreferences != null && s.NotificationPreferences.InAppNotificationsOnly));
        CreateMap<UserModel, UserEntity>()
            .ForMember(d => d.NotificationPreferences, o => o.Ignore());

        CreateMap<UserEntity, UserViewModel>();
        CreateMap<UserViewModel, UserEntity>();
    }

    private static string DisplayName(string? firstName, string? lastName, string? email)
    {
        var fullName = string.Join(' ', new[] { firstName, lastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
        if (!string.IsNullOrWhiteSpace(fullName))
            return fullName.Trim();
        if (!string.IsNullOrWhiteSpace(email) && email.Contains('@'))
            return email.Split('@')[0];
        return string.Empty;
    }
}
