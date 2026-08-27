namespace FarmApp.Shared.Helpers;

public class ActionStorage<T>
{
    private bool _hasBeenExecuted = false;
    private event Func<T>? OnActionExecuted;

    public void Subscribe(Func<T> subscriber)
    {
        OnActionExecuted += subscriber;
        if (_hasBeenExecuted) subscriber.Invoke();
    }

    public void Unsubscribe(Func<T> subscriber)
    {
        OnActionExecuted -= subscriber;
    }

    public void Invoke()
    {
        if (!_hasBeenExecuted)  _hasBeenExecuted = true;
        OnActionExecuted?.Invoke();
    }
}