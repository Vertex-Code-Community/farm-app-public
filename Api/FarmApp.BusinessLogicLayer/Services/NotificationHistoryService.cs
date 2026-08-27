using AutoMapper;
using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.BusinessLogicLayer.Utilities;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.Entities.Entity;
using FarmApp.Models.PushNotification;
using FarmApp.Shared.Constants;

namespace FarmApp.BusinessLogicLayer.Services;

public class NotificationHistoryService : INotificationHistoryService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;

    public NotificationHistoryService(
        INotificationRepository notificationRepository,
        IMapper mapper,
        IUserService userService)
    {
        _notificationRepository = notificationRepository;
        _mapper = mapper;
        _userService = userService;
    }

    public async Task<List<NotificationModel>> GetAllAsync()
    {
        var entities = await _notificationRepository.GetAllAsync();
        return _mapper.Map<List<NotificationModel>>(entities);
    }

    public async Task<List<NotificationModel>> GetMyAsync(string? appVersionSegment, CancellationToken cancellationToken = default)
    {
        var userId = _userService.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return new List<NotificationModel>();

        var comparer = StringComparer.OrdinalIgnoreCase;
        var prefix = NotificationTagExpressionBuilder.UserShopperIdTagPrefix;

        var userShopperTagCandidates = new HashSet<string>(comparer)
        {
            $"{prefix}{userId.Trim()}"
        };

        var profile = await _userService.GetCurrentUserAsync();
        if (!string.IsNullOrWhiteSpace(profile?.Email))
            userShopperTagCandidates.Add($"{prefix}{profile.Email.Trim()}");

        var userHubTags = new HashSet<string>(userShopperTagCandidates, comparer);
        if (!string.IsNullOrWhiteSpace(appVersionSegment))
        {
            var v = appVersionSegment.Trim().ToLowerInvariant();
            if (v is "latest" or "old")
                userHubTags.Add($"{NotificationTagExpressionBuilder.VersionTagPrefix}{v}");
        }

        userHubTags.Add($"{NotificationTagExpressionBuilder.VersionTagPrefix}latest");
        userHubTags.Add($"{NotificationTagExpressionBuilder.VersionTagPrefix}old");

        var entities = await _notificationRepository.GetAllAsync();
        var matched = new List<NotificationEntity>();
        var userTagPrefix = NotificationTagExpressionBuilder.UserShopperIdTagPrefix;

        foreach (var n in entities)
        {
            if (string.Equals(n.TypeOfTargetUser, TargetUserType.All, StringComparison.OrdinalIgnoreCase))
            {
                matched.Add(n);
                continue;
            }

            if (n.Tags is null || n.Tags.Count == 0)
                continue;

            var notifUserTags = new List<string>();
            var notifOtherTags = new List<string>();
            foreach (var raw in n.Tags)
            {
                if (string.IsNullOrWhiteSpace(raw))
                    continue;
                var t = raw.Trim();
                if (t.StartsWith(userTagPrefix, StringComparison.OrdinalIgnoreCase))
                    notifUserTags.Add(t);
                else
                    notifOtherTags.Add(t);
            }

            if (notifUserTags.Count > 0)
            {
                var hit = false;
                foreach (var ut in notifUserTags)
                {
                    if (userShopperTagCandidates.Contains(ut))
                    {
                        hit = true;
                        break;
                    }
                }

                if (!hit)
                    continue;
            }

            var otherOk = true;
            foreach (var ot in notifOtherTags)
            {
                if (!userHubTags.Contains(ot))
                {
                    otherOk = false;
                    break;
                }
            }

            if (otherOk)
                matched.Add(n);
        }

        matched.Sort((a, b) => b.DateTimeOfSend.CompareTo(a.DateTimeOfSend));

        return _mapper.Map<List<NotificationModel>>(matched);
    }

    public async Task<List<NotificationModel>> GetAllDelayedNotificationAsync()
    {
        var entities = await _notificationRepository.GetAllAsync();

        var notifications = entities.Where(x => x.Status == NotificationStatus.Scheduled);

        return _mapper.Map<List<NotificationModel>>(notifications);
    }

    public async Task<long> AddToHistory(NotificationModel notificationModel)
    {
        var entity = _mapper.Map<NotificationEntity>(notificationModel);
        await _notificationRepository.CreateAsync(entity);
        return entity.Id;
    }

    public async Task CompeteAsync(long notificationId)
    {
        var notification = await _notificationRepository.GetByIdForUpdateAsync(notificationId);

        if (notification is null) return;
        notification.Status = NotificationStatus.Sent;
        await _notificationRepository.UpdateAsync(notification);
    }

    public async Task CancelAsync(long notificationId)
    {
        var notification = await _notificationRepository.GetByIdForUpdateAsync(notificationId);

        if (notification is null) return;
        notification.Status = NotificationStatus.Canceled;
        await _notificationRepository.UpdateAsync(notification);
    }

    public async Task UpdateAsync(NotificationModel model)
    {
        var notification = _mapper.Map<NotificationEntity>(model);

        if (notification is null) return;
        await _notificationRepository.UpdateAsync(notification);
    }
}
