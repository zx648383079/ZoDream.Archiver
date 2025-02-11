using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZoDream.KhronosExporter.Models;

namespace ZoDream.KhronosExporter.Converters
{
    internal class TextureInfoConverter : JsonConverter<TextureInfo>
    {
        public override TextureInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return null;
            //options.PropertyNamingPolicy?.ConvertName(reader.GetString());
            //options.GetConverter(typeToConvert).Read(ref reader, typeToConvert, options);
        }

        public override void Write(Utf8JsonWriter writer, TextureInfo value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
