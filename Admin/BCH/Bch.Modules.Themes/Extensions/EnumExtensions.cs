namespace Bch.Modules.Themes.Extensions;

public static class EnumExtensions
{
    public static TR? GetValue<TR, T>(this Enum value, Func<T, TR> predicate) where T : Attribute
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        var descriptionAttributes = fieldInfo!.GetCustomAttributes(typeof(T), false) as T[];

        return (descriptionAttributes is not null && descriptionAttributes.Length > 0)
            ? predicate(descriptionAttributes[0])
            : default;
    }
}


