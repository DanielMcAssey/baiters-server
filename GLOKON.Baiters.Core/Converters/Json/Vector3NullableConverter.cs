using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GLOKON.Baiters.Core.Converters.Json
{
    public sealed class Vector3NullableConverter : JsonConverter<Vector3?>
    {
        public override Vector3? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var rawVal = reader.GetString();

            if (rawVal != null)
            {
                return JsonSerializer.Deserialize<Vector3?>(rawVal, JsonOptions.ConverterDefault);
            }

            return Vector3.Zero; // Couldnt parse, default value
        }

        public override void Write(Utf8JsonWriter writer, Vector3? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("x");
                writer.WriteNumberValue(value.Value.X);
                writer.WritePropertyName("y");
                writer.WriteNumberValue(value.Value.Y);
                writer.WritePropertyName("z");
                writer.WriteNumberValue(value.Value.Z);
                writer.WriteEndObject();
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
