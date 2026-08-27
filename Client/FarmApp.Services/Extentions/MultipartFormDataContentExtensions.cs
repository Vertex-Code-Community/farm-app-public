using System.Collections;
using System.Reflection;

namespace FarmApp.Services.Extentions;

public static class MultipartFormDataContentExtensions
{
    public static void AddModel(this MultipartFormDataContent content, object model)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(model);

        var props = model.GetType()
                         .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                         .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

        foreach (var prop in props)
        {
            var value = prop.GetValue(model);
            if (value is null) continue;

            var propName = prop.Name;

            // If it's an IEnumerable (but not string), add each item separately
            if (value is IEnumerable enumerable and not string)
            {
                foreach (var item in enumerable)
                {
                    if (item == null) continue;
                    content.Add(new StringContent(ConvertToString(item)), propName);
                }

                continue;
            }

            content.Add(new StringContent(ConvertToString(value)), propName);
        }
    }

    private static string ConvertToString(object value)
    {
        const string format = "yyyy-MM-ddTHH:mm:ss";
        return value switch
        {
            DateTime dt => dt.ToString(format),
            DateTimeOffset dto => dto.ToString(format),
            bool b => b ? "true" : "false",
            Enum e => Convert.ToInt64(e).ToString(),
            _ => value.ToString() ?? string.Empty
        };
    }
}
