using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace GLOKON.Baiters.Core.Converters.Json
{
    public sealed class Vector3NullableConverter : JsonConverter<Vector3?>
    {
        public override Vector3? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (JsonNode.Parse(ref reader) is JsonObject obj &&
                obj.TryGetPropertyValue("x", out var xProp) && xProp != null &&
                obj.TryGetPropertyValue("y", out var yProp) && yProp != null &&
                obj.TryGetPropertyValue("z", out var zProp) && zProp != null)
            {
                return new Vector3((float)xProp, (float)yProp, (float)zProp);
            }

            return Vector3.Zero;
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
