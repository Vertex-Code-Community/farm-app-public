using Newtonsoft.Json;

namespace FarmApp.Shared.JsonConverters;

public class EmptyStringToNullConverter<T> : JsonConverter where T : class
{
    public override bool CanConvert(Type objectType) => objectType == typeof(T);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String && string.IsNullOrEmpty((string?)reader.Value))
            return null; // If empty string, return null

        // Otherwise, we proceed to deserialize the object, but we don't use the converter recursively
        if (reader.TokenType == JsonToken.Null)
            return null;

        // IMPORTANT: This deserializes without using the current converter again (prevents recursion)
        return JsonSerializer.CreateDefault().Deserialize(reader, objectType);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value); // Normal serialization
    }
}