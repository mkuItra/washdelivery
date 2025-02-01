using System.Text.Json;
using System.Text.Json.Serialization;

namespace System.Text.Json.Serialization
{
    public class JsonDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateTime = reader.GetDateTime();
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Convert to UTC before writing
            if (value.Kind == DateTimeKind.Local)
            {
                value = value.ToUniversalTime();
            }
            else if (value.Kind == DateTimeKind.Unspecified)
            {
                value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }

            writer.WriteStringValue(value.ToString("O")); // ISO 8601 format
        }
    }
} 