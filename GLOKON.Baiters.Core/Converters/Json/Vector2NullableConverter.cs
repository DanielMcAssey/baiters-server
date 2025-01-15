using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace GLOKON.Baiters.Core.Converters.Json
{
    public sealed class Vector2NullableConverter : JsonConverter<Vector2?>
    {
        public override Vector2? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (JsonNode.Parse(ref reader) is JsonObject obj &&
                obj.TryGetPropertyValue("x", out var xProp) && xProp != null &&
                obj.TryGetPropertyValue("y", out var yProp) && yProp != null)
            {
                return new Vector2((float)xProp, (float)yProp);
            }

            return Vector2.Zero;
        }

        public override void Write(Utf8JsonWriter writer, Vector2? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("x");
                writer.WriteNumberValue(value.Value.X);
                writer.WritePropertyName("y");
                writer.WriteNumberValue(value.Value.Y);
                writer.WriteEndObject();
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
