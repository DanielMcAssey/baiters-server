using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace GLOKON.Baiters.Core.Converters.Json
{
    public sealed class Vector3Converter : JsonConverter<Vector3>
    {
        public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (JsonNode.Parse(ref reader) is JsonObject obj &&
                obj.TryGetPropertyValue("x", out var xProp) && xProp != null &&
                obj.TryGetPropertyValue("y", out var yProp) && yProp != null &&
                obj.TryGetPropertyValue("z", out var zProp) && zProp != null)
            {
                return new Vector3((float)xProp, (float)yProp, (float)zProp);
            }

            return Vector3.Zero;
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
