using FarmApp.Shared.JsonConverters;
using Newtonsoft.Json;

namespace FarmApp.Shared.Helpers;

public class JsonConvertHelper
{
    public static T? DeserializeJsonWithEmptyStringToNull<T>(string json) where T : class
    {
        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new EmptyStringToNullConverter<T>());

        try
        {
            return JsonConvert.DeserializeObject<T?>(json, settings);
        }
        catch
        {
            return default;
        }
    }
}