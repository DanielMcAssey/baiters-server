using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GLOKON.Baiters.Core.Converters.Json
{
    public sealed class Vector3Converter : JsonConverter<Vector3>
    {
        public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var rawVal = reader.GetString();

            if (rawVal != null)
            {
                return JsonSerializer.Deserialize<Vector3>(rawVal, JsonOptions.ConverterDefault);
            }

            return Vector3.Zero; // Couldnt parse, default value
        }

        public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteNumberValue(value.X);
            writer.WritePropertyName("y");
            writer.WriteNumberValue(value.Y);
            writer.WritePropertyName("z");
            writer.WriteNumberValue(value.Z);
            writer.WriteEndObject();
        }
    }
}
