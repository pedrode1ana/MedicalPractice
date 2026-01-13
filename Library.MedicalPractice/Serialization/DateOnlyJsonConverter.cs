using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Library.MedicalPractice.Serialization;

public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";

    public override void WriteJson(JsonWriter writer, DateOnly value, JsonSerializer serializer) =>
        writer.WriteValue(value.ToString(Format));

    public override DateOnly ReadJson(JsonReader reader, Type objectType, DateOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String && reader.Value is string s && DateOnly.TryParse(s, out var date))
        {
            return date;
        }
        return default;
    }
}
