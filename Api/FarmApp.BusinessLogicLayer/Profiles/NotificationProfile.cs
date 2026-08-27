using AutoMapper;
using FarmApp.BusinessLogicLayer.Utilities;
using FarmApp.Entities.Entity;
using FarmApp.Models.PushNotification;
using FarmApp.Models.PushNotificationHistory;

namespace FarmApp.BusinessLogicLayer.Profiles;

public class NotificationProfile : Profile
{
    public NotificationProfile()
    {
        CreateMap<NotificationEntity, NotificationModel>()
            .ForMember(d => d.Tags,
                opt => opt.MapFrom(s => NotificationTagExpressionBuilder.FromEntityTags(s.Tags)));
        CreateMap<NotificationModel, NotificationEntity>()
            .ForMember(d => d.Tags, opt => opt.MapFrom(s => NotificationTagExpressionBuilder.ToEntityTags(s)));

        CreateMap<NotificationHistoryItemModel, NotificationEntity>();
        CreateMap<NotificationEntity, NotificationHistoryItemModel>();
    }
}