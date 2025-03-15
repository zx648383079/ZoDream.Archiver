using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZoDream.KhronosExporter.Models;
using ZoDream.Shared.Converters;

namespace ZoDream.KhronosExporter.Converters
{
    internal class EnumNameConverter<T> : JsonConverter<T>
        where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Enum.TryParse<T>(StringConverter.Studly(reader.GetString()), out var res) ? res : Enum.GetValues<T>().First();
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value is AnimationInterpolationMode)
            {
                writer.WriteStringValue(Enum.GetName(value)?.ToUpper());
                return;
            }
            writer.WriteStringValue(Enum.GetName(value)?.ToLower());
        }
    }
}
