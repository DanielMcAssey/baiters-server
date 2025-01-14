using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GLOKON.Baiters.Core.Converters.Json
{
    public sealed class Vector2Converter : JsonConverter<Vector2>
    {
        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var rawVal = reader.GetString();

            if (rawVal != null)
            {
                return JsonSerializer.Deserialize<Vector2>(rawVal, JsonOptions.ConverterDefault);
            }

            return Vector2.Zero; // Couldnt parse, default value
        }

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteNumberValue(value.X);
            writer.WritePropertyName("y");
            writer.WriteNumberValue(value.Y);
            writer.WriteEndObject();
        }
    }
}
