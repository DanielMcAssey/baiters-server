using System.Text.Json;

namespace GLOKON.Baiters.Core.Converters.Json
{
    public static class JsonOptions
    {
        public static JsonNamingPolicy NamingPolicy { get; } = JsonNamingPolicy.CamelCase;

        internal static JsonSerializerOptions ConverterDefault { get; } = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = NamingPolicy,
        };

        public static JsonSerializerOptions Default { get; } = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = NamingPolicy,
            Converters =
            {
                new Vector2Converter(),
                new Vector2NullableConverter(),
                new Vector3Converter(),
                new Vector3NullableConverter(),
            },
        };
    }
}
