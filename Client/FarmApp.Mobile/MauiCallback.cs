namespace FarmApp.Mobile;

public static class MauiCallback
{
    public static Func<Task>? OnTestPayment;

    public static void InvokeTestPayment()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            OnTestPayment?.Invoke();
        });
    }
}