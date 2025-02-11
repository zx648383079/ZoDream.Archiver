using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZoDream.KhronosExporter.Models;
using ZoDream.Shared.Converters;

namespace ZoDream.KhronosExporter.Converters
{
    internal class PropertyPathConverter : JsonConverter<PropertyPath>
    {
        public override PropertyPath Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Enum.TryParse<PropertyPath>(StringConverter.Studly(reader.GetString()), out var res) ? res : PropertyPath.Translation;
        }

        public override void Write(Utf8JsonWriter writer, PropertyPath value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Enum.GetName(value)?.ToLower());
        }
    }
}
