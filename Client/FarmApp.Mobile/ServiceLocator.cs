namespace FarmApp.Mobile;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> Services = new();

    public static void Register<T>(T service)
    {
        if (service != null) Services.Add(typeof(T), service);
    }

    public static T? Resolve<T>()
    {
        if (Services.TryGetValue(typeof(T), out var service))
        {
            return (T)service;
        }

        return default;
    }
}
