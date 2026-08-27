using AndroidX.Activity.Result;

namespace FarmApp.Mobile.Platforms.Android;

public class ActivityResultCallback : Java.Lang.Object, IActivityResultCallback
{
    private readonly Action<ActivityResult> _onResult;

    public ActivityResultCallback(Action<ActivityResult> onResult)
    {
        _onResult = onResult;
    }

    public void OnActivityResult(Java.Lang.Object result)
    {
        _onResult((ActivityResult)result);
    }
}