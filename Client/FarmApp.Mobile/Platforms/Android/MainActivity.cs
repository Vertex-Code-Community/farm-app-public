using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Tasks;
using Android.OS;
using Android.Views;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using AndroidX.Core.View;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using FarmApp.Mobile.Platforms.Android;
using FarmApp.Mobile.Services.Interfaces;
using FarmApp.Services.Providers;
using Firebase.Messaging;
using AndroidReal = Microsoft.Maui.Controls.PlatformConfiguration.Android;
using View = Android.Views.View;

namespace FarmApp.Mobile;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity, IOnApplyWindowInsetsListener, IOnSuccessListener
{
    public static ActivityResultLauncher? MediaChooserLauncher;
    public static Action<ActivityResult>? MediaChooserCallback;
    IPushNotificationsMessageService _notificationMessageService;
    IDeviceInstallationService _deviceInstallationService;
    
    IPushNotificationsMessageService NotificationMessageService =>
        _notificationMessageService ?? (_notificationMessageService = IPlatformApplication.Current.Services.GetService<IPushNotificationsMessageService>());

    IDeviceInstallationService DeviceInstallationService =>
        _deviceInstallationService ?? (_deviceInstallationService = IPlatformApplication.Current.Services.GetService<IDeviceInstallationService>());
    
    public void OnSuccess(Java.Lang.Object? result)
    {
        if (result is null) return;

        var token = result.ToString();
        if (!string.IsNullOrWhiteSpace(token))
            DeviceInstallationService.DeviceTokenTcs.TrySetResult(token);
    }
    
    void ProcessNotificationsAction(Intent? intent)
    {
        if (intent is null) return;
        
        try
        {
            if (!intent.HasExtra("body")) return;
            
            var title = intent.GetStringExtra("title");
            var message = intent.GetStringExtra("body");

            if (!string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(title))
                NotificationMessageService.Invoke(title, message);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
    }
    
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        try
        {
            if (Firebase.FirebaseApp.Instance == null)
                Firebase.FirebaseApp.InitializeApp(this);
        }
        catch
        {
            Firebase.FirebaseApp.InitializeApp(this);
        }

        try
        {
            if (DeviceInstallationService.NotificationsSupported)
                FirebaseMessaging.Instance.GetToken().AddOnSuccessListener(this);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Firebase] Failed to get token: {ex.Message}");
        }

        ProcessNotificationsAction(Intent);
        
        RequestedOrientation = ScreenOrientation.Portrait;
        WindowCompat.SetDecorFitsSystemWindows(Window, false);
        ViewCompat.SetOnApplyWindowInsetsListener(Window.DecorView, this);
        MediaChooserLauncher = RegisterForActivityResult(
            new ActivityResultContracts.StartActivityForResult(),
            new ActivityResultCallback(result =>
            {
                MediaChooserCallback?.Invoke(result);
            })
        );
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? intent)
    {
        base.OnActivityResult(requestCode, resultCode, intent);

        if (requestCode == 999)
        {
            WeakReferenceMessenger.Default.Send(
            new ValueChangedMessage<string>(
            resultCode == Result.Ok
            ? "AuthorizationComplete"
            : "AuthorizationCancelled"));
        }
        var picker = IPlatformApplication.Current?.Services.GetService<IGalleryPickerService>() as AndroidGalleryPickerService;

        picker?.HandleActivityResult(requestCode, resultCode, intent);
    }

    public WindowInsetsCompat OnApplyWindowInsets(View view, WindowInsetsCompat windowInsets)
    {
        var systemInsets = windowInsets.GetInsets(WindowInsetsCompat.Type.SystemGestures());
        var statusBarInsets = windowInsets.GetInsets(WindowInsetsCompat.Type.StatusBars());
        
        // Apply the insets as padding to the view. Here, set all the dimensions
        // as appropriate to your layout. You can also update the view's margin if
        // more appropriate.
        view.SetPadding(0, 0, 0, 0);

        Console.WriteLine($"Left: {systemInsets.Left}, Top: {statusBarInsets.Top}, Right: {systemInsets.Right}, Bottom: {systemInsets.Bottom}");

        var density = Resources?.DisplayMetrics?.Density ?? 1;
        
        ScreenOffsetProvider.Top = (int) (statusBarInsets.Top / density);
        ScreenOffsetProvider.Bottom = (int) (systemInsets.Bottom / density);
        ScreenOffsetProvider.Left = systemInsets.Left;
        ScreenOffsetProvider.Right = systemInsets.Right;
        ScreenOffsetProvider.Density = density;
        ScreenOffsetProvider.ScreenWidth = Resources?.DisplayMetrics?.WidthPixels ?? 0;
        ScreenOffsetProvider.ScreenHeight = (Resources?.DisplayMetrics?.HeightPixels ?? 0) + statusBarInsets.Top + systemInsets.Bottom;
        
        Console.WriteLine($"DENSITY: {density}");

        // if (Resources?.DisplayMetrics != null)
        // {
        //     var topPx = (int) (insets.Top * Resources.DisplayMetrics.Density);
        //     Console.WriteLine($"TOP PX {topPx}");
        //     Console.WriteLine($"TOP PX {topPx}");
        // }
        
        // Return CONSUMED if you don't want the window insets to keep passing down
        // to descendant views.
        return windowInsets;
    }

    public override bool DispatchTouchEvent(MotionEvent? ev) {
        var continueProcessing = ev is not null && SystemEventsProvider.InvokeTouch(ev);
        
        if (continueProcessing)
            base.DispatchTouchEvent(ev);
        
        if (ev is not null && ev.ActionMasked == MotionEventActions.Up)
            SystemEventsProvider.InvokeLastTouchLifted();

        return continueProcessing;
    }
}