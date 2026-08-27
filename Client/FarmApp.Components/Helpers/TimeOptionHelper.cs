using FarmApp.Shared.Enums;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Helpers
{
    public static class TimeOptionHelper
    {
        private static Dictionary<NotificationOffset, TimeOptionModel>? _cache;

        public static IReadOnlyList<TimeOptionModel> GetAll(IStringLocalizer localizer)
        {
            EnsureCache(localizer);
            return _cache!.Values.ToList();
        }

        public static TimeOptionModel? GetByDuration(NotificationOffset duration, IStringLocalizer localizer)
        {
            EnsureCache(localizer);
            return _cache!.TryGetValue(duration, out var value) ? value : null;
        }

        private static void EnsureCache(IStringLocalizer localizer)
        {
            if (_cache != null)
                return;

            _cache = new Dictionary<NotificationOffset, TimeOptionModel>
        {
            { NotificationOffset.FiveMinutes, new() { Label = localizer["Notifications_Remind_5-min"], Duration = NotificationOffset.FiveMinutes } },
            { NotificationOffset.TenMinutes, new() { Label = localizer["Notifications_Remind_10-min"], Duration = NotificationOffset.TenMinutes } },
            { NotificationOffset.FifteenMinutes, new() { Label = localizer["Notifications_Remind_15-min"], Duration = NotificationOffset.FifteenMinutes } },
            { NotificationOffset.TwentyMinutes, new() { Label = localizer["Notifications_Remind_20-min"], Duration = NotificationOffset.TwentyMinutes } },
            { NotificationOffset.ThirtyMinutes, new() { Label = localizer["Notifications_Remind_30-min"], Duration = NotificationOffset.ThirtyMinutes } },
            { NotificationOffset.OneHour, new() { Label = localizer["Notifications_Remind_1-hour"], Duration = NotificationOffset.OneHour } },
            { NotificationOffset.TwoHours, new() { Label = localizer["Notifications_Remind_2-hours"], Duration = NotificationOffset.TwoHours } },
            { NotificationOffset.OneDay, new() { Label = localizer["Notifications_Remind_1-day"], Duration = NotificationOffset.OneDay } },
            { NotificationOffset.OneWeek, new() { Label = localizer["Notifications_Remind_1-week"], Duration = NotificationOffset.OneWeek } }
        };
        }
    }
}
