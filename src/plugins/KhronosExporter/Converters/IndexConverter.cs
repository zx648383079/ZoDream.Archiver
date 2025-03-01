using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ZoDream.KhronosExporter.Converters
{
    internal class IndexConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TryGetInt32(out var i) ? i : -1;
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            if (value < 0)
            {
                writer.WriteNullValue();
            } else
            {
                writer.WriteNumberValue(value);
            }
        }
    }
}
