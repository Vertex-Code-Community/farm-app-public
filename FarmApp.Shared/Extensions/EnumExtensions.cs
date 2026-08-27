using System.ComponentModel;

namespace FarmApp.Shared.Extensions;

public static class EnumExtensions
{
    public static List<string> GetEnumNames<T>() where T : Enum
    {
        if (!typeof(T).IsEnum) throw new ArgumentException($"{typeof(T).FullName} is not an enum type.");

        return Enum.GetNames(typeof(T)).ToList();
    }

    public static string GetDescription(this Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : value.ToString();
    }
    
    public static R GetValue<R, T>(this Enum value, Func<T, R> predicate) where T : Attribute
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        if (fieldInfo is null) return default!;

        var descriptionAttributes = fieldInfo.GetCustomAttributes(typeof(T), false) as T[];

        return (descriptionAttributes is not null && descriptionAttributes.Length > 0) ? predicate(descriptionAttributes[0]) : default!;
    }
    
    public static T? GetAttribute<T>(this Enum value) where T : Attribute
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        var descriptionAttributes = fieldInfo?.GetCustomAttributes(typeof(T), false) as T[];

        return descriptionAttributes?.FirstOrDefault();
    }
}
