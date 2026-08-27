using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using FarmApp.Mobile;
using FarmApp.Mobile.Services.Interfaces;
using FarmApp.Services.Services.Interfaces;
using Firebase.Messaging;

using AUri = global::Android.Net.Uri;

namespace FarmApp.Mobile.Platforms.Android;

[Service(Exported = false)]
[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
public class PushNotificationFirebaseMessagingService : FirebaseMessagingService
{
    const string CHANNEL_NAME = "General";
	
    IDeviceInstallationService _deviceInstallationService;

    IDeviceInstallationService DeviceInstallationService =>
        _deviceInstallationService ?? (_deviceInstallationService =
            IPlatformApplication.Current.Services.GetService<IDeviceInstallationService>());

    static INotificationDisplayPreferences? ResolveDisplayPreferences() =>
        IPlatformApplication.Current?.Services?.GetService<INotificationDisplayPreferences>();

    public override void OnNewToken(string token)
    {
        System.Diagnostics.Debug.WriteLine($"##FCM_TOKEN## {token}");
        DeviceInstallationService.DeviceTokenTcs.TrySetResult(token);
    }

    public override void OnMessageReceived(RemoteMessage message)
    {
        base.OnMessageReceived(message);

        var data = message.Data ?? new Dictionary<string, string>();

        var title = data.TryGetValue("title", out var t) ? t : message.GetNotification()?.Title ?? "Notification";
        var body = data.TryGetValue("body", out var b) ? b : message.GetNotification()?.Body ?? string.Empty;
        var url = data.TryGetValue("url", out var u) ? u : message.GetNotification()?.Link?.ToString();

        //NotificationMessageService?.Invoke(title, body);

        ShowSystemNotification(title, body, url);
    }

    private static string BuildChannelId(bool sound, bool vibration) =>
        sound
            ? (vibration ? "farm_push_snd_vib" : "farm_push_snd_novib")
            : (vibration ? "farm_push_nosnd_vib" : "farm_push_nosnd_novib");

    private void ShowSystemNotification(string title, string body, string? url)
    {
        var prefs = ResolveDisplayPreferences();
        var soundOn = prefs?.IsSoundEnabled() ?? true;
        var vibrationOn = prefs?.IsVibrationEnabled() ?? false;

        var channelId = BuildChannelId(soundOn, vibrationOn);
        EnsureChannel(channelId, CHANNEL_NAME, soundOn, vibrationOn);

		// Content intent for tapping the notification body -> open app
		var contentIntent = new Intent(this, typeof(MainActivity))
			.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);

        var requestCode = (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() & 0x7FFFFFFF);
		var pendingContent = PendingIntent.GetActivity(
			this, requestCode, contentIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

		// Optional button intent -> open URL in browser
		PendingIntent? pendingOpenUrl = null;
		var hasUrl = !string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.Absolute, out _);
		if (hasUrl)
		{
			var openUrlIntent = new Intent(Intent.ActionView, AUri.Parse(url))
				.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
			var openUrlRequestCode = (int)((DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 1) & 0x7FFFFFFF);
			pendingOpenUrl = PendingIntent.GetActivity(
				this, openUrlRequestCode, openUrlIntent,
				PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
		}

		// Resolve icon resources dynamically to avoid hardcoded Resource.* symbols
		// Large icon in our custom layout: prefer "appicon" drawable, then "maui_splash", then launcher, then system fallback
		var appIconId = Resources.GetIdentifier("appicon", "drawable", PackageName);
		if (appIconId == 0)
			appIconId = Resources.GetIdentifier("maui_splash", "drawable", PackageName);
		if (appIconId == 0)
			appIconId = Resources.GetIdentifier("ic_launcher", "mipmap", PackageName);
		if (appIconId == 0)
			appIconId = Resources.GetIdentifier("ic_dialog_info", "drawable", "android"); // system fallback

		// Small notification icon shown in the status bar:
		// Prefer explicit "appicon" (MAUI generates this in mipmap from Resources/AppIcon/appicon.png),
		// otherwise a white silhouette "ic_notification", then launcher icons, then system default.
		var smallIconId = Resources.GetIdentifier("ic_notification", "drawable", PackageName);
		if (smallIconId == 0)
			smallIconId = Resources.GetIdentifier("ic_notification", "mipmap", PackageName);
		if (smallIconId == 0)
			smallIconId = Resources.GetIdentifier("appicon", "mipmap", PackageName);
		if (smallIconId == 0)
			smallIconId = Resources.GetIdentifier("appicon", "drawable", PackageName);
		if (smallIconId == 0)
			smallIconId = Resources.GetIdentifier("ic_launcher", "mipmap", PackageName);
		if (smallIconId == 0)
			smallIconId = Resources.GetIdentifier("ic_launcher_round", "mipmap", PackageName);
		if (smallIconId == 0)
			smallIconId = Resources.GetIdentifier("ic_dialog_info", "drawable", "android");

		// Build custom notification layout (dynamic ids to avoid Resource.* compile-time bindings)
		RemoteViews? collapsed = null;
		var layoutId = Resources.GetIdentifier("notification_custom", "layout", PackageName);
		if (layoutId != 0)
		{
			collapsed = new RemoteViews(PackageName, layoutId);

			var titleViewId = Resources.GetIdentifier("txtTitle", "id", PackageName);
			var bodyViewId = Resources.GetIdentifier("txtBody", "id", PackageName);
			var buttonViewId = Resources.GetIdentifier("btnOpenInBrowser", "id", PackageName);

			if (titleViewId != 0)
				collapsed.SetTextViewText(titleViewId, title);
			if (bodyViewId != 0)
				collapsed.SetTextViewText(bodyViewId, body);

			if (buttonViewId != 0)
			{
				if (hasUrl && pendingOpenUrl is not null)
				{
					collapsed.SetOnClickPendingIntent(buttonViewId, pendingOpenUrl);
					collapsed.SetViewVisibility(buttonViewId, ViewStates.Visible);
				}
				else
				{
					collapsed.SetViewVisibility(buttonViewId, ViewStates.Gone);
				}
			}
		}

		// Build the notification
		var builder = new NotificationCompat.Builder(this, channelId)
			.SetSmallIcon(smallIconId == 0 ? Resources.GetIdentifier("ic_dialog_info", "drawable", "android") : smallIconId)
			.SetContentIntent(pendingContent)
			.SetAutoCancel(true);

		if (Build.VERSION.SdkInt < BuildVersionCodes.O)
		{
			var defaults = (NotificationDefaults)0;
			if (soundOn)
				defaults |= NotificationDefaults.Sound;
			if (vibrationOn)
				defaults |= NotificationDefaults.Vibrate;
			builder.SetDefaults((int)defaults);
			if (!soundOn)
				builder.SetSound(null);
			if (!vibrationOn)
				builder.SetVibrate(null);
		}

		if (collapsed is not null)
		{
			builder.SetCustomContentView(collapsed)
			       .SetCustomBigContentView(collapsed);
		}
		else
		{
			// Graceful fallback to default template if custom layout is not available
			builder.SetContentTitle(title)
			       .SetContentText(body)
			       .SetStyle(new NotificationCompat.BigTextStyle().BigText(body));
			if (pendingOpenUrl is not null && hasUrl)
			{
				builder.AddAction(0, "Open in Browser", pendingOpenUrl);
			}
		}

		var notif = builder.Build();

        NotificationManagerCompat.From(this)?.Notify(requestCode, notif);
    }

    private void EnsureChannel(string id, string name, bool soundOn, bool vibrationOn)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26))
            return;

        var mgr = (NotificationManager)GetSystemService(NotificationService)!;
        if (mgr.GetNotificationChannel(id) != null)
            return;

        var ch = new NotificationChannel(id, name, NotificationImportance.Default)
        {
            Description = "General push notifications"
        };

        if (soundOn)
        {
            var uri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
            var attrs = new AudioAttributes.Builder()
                .SetUsage(AudioUsageKind.Notification)!
                .Build();
            ch.SetSound(uri, attrs);
        }
        else
            ch.SetSound(null, null);

        ch.EnableVibration(vibrationOn);
        if (vibrationOn)
            ch.SetVibrationPattern(new long[] { 0, 250, 250, 250 });

        mgr.CreateNotificationChannel(ch);
    }
}