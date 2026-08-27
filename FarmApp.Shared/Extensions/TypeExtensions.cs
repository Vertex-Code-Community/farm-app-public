namespace FarmApp.Shared.Extensions;

public static class TypeExtensions
{
    public static R GetValue<R, T>(this Type type, Func<T, R> predicate) where T : Attribute
    {
        var descriptionAttributes = type.GetCustomAttributes(typeof(T), false) as T[];

        return (descriptionAttributes is not null && descriptionAttributes.Length > 0) ? predicate(descriptionAttributes[0]) : default!;
    }
}