using System.Text.RegularExpressions;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.Models.PushNotification;
using FarmApp.Shared.Enums.PushNotification;
using FarmApp.ViewModels.Notifications;
using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Components.NotificationBlock
{
    public partial class NotificationBlockComponent
    {
        [Inject] public required INavigationService NavigationService { get; set; }
        [Parameter] public required UserNotificationViewModel Notification { get; set; }

        [Parameter] public bool ShowFull { get; set; } = false;

        [Parameter] public int? AnimationIndex { get; set; }

        private NotificationIconStyle _iconStyles = new("var(--notification-icon-bg)", "original", "_content/FarmApp.Components/img/shared/bell.svg");

        protected override void OnParametersSet()
        {
            _iconStyles = ResolveIconStyle(Notification);
            base.OnParametersSet();
        }

        private static NotificationIconStyle ResolveIconStyle(UserNotificationViewModel n)
        {
            if (!string.IsNullOrWhiteSpace(n.IconSrc))
            {
                var bg = string.IsNullOrWhiteSpace(n.IconBackground)
                    ? "var(--notification-icon-bg)"
                    : n.IconBackground!;
                var color = string.IsNullOrWhiteSpace(n.IconColor) ? "original" : n.IconColor!;
                return new NotificationIconStyle(bg, color, n.IconSrc.Trim());
            }

            if (n.NotificationKind == NotificationKind.Weather)
            {
                var fromText = TryResolveWeatherIconFromText(n.Title, n.Content);
                if (fromText is not null)
                    return fromText;
            }

            return ResolveNotificationIconFromKind(n.NotificationKind);
        }

        /// <summary>
        /// Picks a weather SVG + tint from title/message (English keywords), aligned with mock notification styling.
        /// </summary>
        private static NotificationIconStyle? TryResolveWeatherIconFromText(string? title, string? content)
        {
            var text = $"{title}\n{content}";
            if (string.IsNullOrWhiteSpace(text))
                return null;

            static bool HasWord(string haystack, string pattern) =>
                Regex.IsMatch(haystack, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            if (HasWord(text, @"\b(snow|snowy|blizzard|sleet|hail|frost|frosty|icy|ice)\b"))
                return new NotificationIconStyle("#D5EAFF", "original", NotificationAssetPaths.Weather.Snowflake);

            if (HasWord(text, @"\b(drought|arid|desert)\b"))
                return new NotificationIconStyle("#FDF2CA", "original", NotificationAssetPaths.Weather.Cactus);

            if (HasWord(text, @"\b(humid|humidity|fog|foggy|mist|misty|dew)\b"))
                return new NotificationIconStyle("#EEE7FF", "original", NotificationAssetPaths.Weather.Drop);

            if (HasWord(text, @"\b(thunder|thunderstorm|lightning)\b"))
                return new NotificationIconStyle("#E6F3FF", "original", NotificationAssetPaths.Weather.CloudRain);

            if (HasWord(text, @"\b(rain|rainy|raining|shower|showers|drizzle|precipitation|wet)\b"))
                return new NotificationIconStyle("#E6F3FF", "original", NotificationAssetPaths.Weather.CloudRain);

            if (HasWord(text, @"\b(wind|windy|gale|gust|breeze)\b"))
                return new NotificationIconStyle("#F5F3FF", "original", NotificationAssetPaths.Weather.Wind);

            if (HasWord(text, @"\b(sun|sunny|clear\s+sky|bright)\b"))
                return new NotificationIconStyle("#FFEEDB", "original", NotificationAssetPaths.Weather.Sun);

            if (HasWord(text, @"\b(heat|hot|scorch|scorching|heatwave|heat\s+stress)\b"))
                return new NotificationIconStyle("#FFDED8", "original", NotificationAssetPaths.Weather.TemperatureHot);

            if (HasWord(text, @"\b(cold|freez|freezing|frigid|chill|chilly|subzero)\b"))
                return new NotificationIconStyle("#C2E1FF", "original", NotificationAssetPaths.Weather.TemperatureCold);

            return null;
        }

        private static NotificationIconStyle ResolveNotificationIconFromKind(NotificationKind kind)
        {
            return kind switch
            {
                NotificationKind.General => new NotificationIconStyle(
                    "var(--notification-icon-bg)",
                    "original",
                    NotificationAssetPaths.Shared.Bell),

                NotificationKind.Weather => new NotificationIconStyle(
                    "#E3F2FD",
                    "original",
                    NotificationAssetPaths.Weather.PartlyCloudy),

                NotificationKind.FieldReminder => new NotificationIconStyle(
                    "#F3E8FF",
                    "var(--component-icon)",
                    NotificationAssetPaths.Notes.Calendar),

                NotificationKind.System => new NotificationIconStyle(
                    "#FFF4E5",
                    "var(--component-icon)",
                    NotificationAssetPaths.User.AlertTriangle),

                NotificationKind.Maintenance => new NotificationIconStyle(
                    "#E8F4FC",
                    "var(--component-icon)",
                    NotificationAssetPaths.User.Tool),

                NotificationKind.Marketing => new NotificationIconStyle(
                    "#E8F5E9",
                    "original",
                    NotificationAssetPaths.Shared.Leaf),

                _ => new NotificationIconStyle(
                    "var(--notification-icon-bg)",
                    "original",
                    NotificationAssetPaths.Shared.Bell)
            };
        }

        private void NavigateToViewNotification(UserNotificationViewModel Notification)
        {
            NavigationService.NavigateTo(Constants.ClientRoutes.ViewNotificationPage, new Dictionary<string, object>
            {
                { "Notification", Notification }
            });
        }

        private string GetNotificationDateString(DateTime date)
        {
            DateTime today = DateTime.Today;

            if (date.Date == today)
            {
                return "Today";
            }
            else if (date.Date == today.AddDays(-1))
            {
                return "Yesterday";
            }
            else if (date.Year == today.Year)
            {
                return date.ToString("dd MMM");
            }
            else
            {
                return date.ToString("dd MMM yyyy");
            }
        }

    }
}

public record NotificationIconStyle(string BackgroundColor, string Color, string Source);