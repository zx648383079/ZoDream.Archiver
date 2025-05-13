using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZoDream.KhronosExporter.Models;

namespace ZoDream.KhronosExporter.Converters
{
    internal class ExtensionConverter : JsonConverter<Dictionary<string, object>>
    {

        private readonly Dictionary<string, Type> _extensionItems = new()
        {
            {AnimationPointer.ExtensionName, typeof(AnimationPointer) },
            {LightsPunctual.ExtensionName, typeof(LightsPunctual) },
            {MaterialPBRSpecularGlossiness.ExtensionName, typeof(MaterialPBRSpecularGlossiness) },
            {MaterialsAnisotropy.ExtensionName, typeof(MaterialsAnisotropy) },
            {MaterialsClearCoat.ExtensionName, typeof(MaterialsClearCoat) },
            {MaterialsDiffuseTransmission.ExtensionName, typeof(MaterialsDiffuseTransmission) },
            {MaterialsDispersion.ExtensionName, typeof(MaterialsDispersion) },
            {MaterialsEmissiveStrength.ExtensionName, typeof(MaterialsEmissiveStrength) },
            {MaterialsIOR.ExtensionName, typeof(MaterialsIOR) },
            {MaterialsIridescence.ExtensionName, typeof(MaterialsIridescence) },
            {MaterialsSheen.ExtensionName, typeof(MaterialsSheen) },
            {MaterialsSpecular.ExtensionName, typeof(MaterialsSpecular) },
            {MaterialsTransmission.ExtensionName, typeof(MaterialsTransmission) },
            {MaterialsUnlit.ExtensionName, typeof(MaterialsUnlit) },
            {MaterialsVariants.ExtensionName, typeof(MaterialsVariants) },
            {MaterialsVolume.ExtensionName, typeof(MaterialsVolume) },
            {TextureTransform.ExtensionName, typeof(TextureTransform) },

        };

        public override Dictionary<string, object>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }
            var data = new Dictionary<string, object>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return data;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();
                    reader.Read();
                    if (!_extensionItems.TryGetValue(propertyName, out var extensionType))
                    {
                        extensionType = typeToConvert;
                    }
                    data.Add(propertyName, JsonSerializer.Deserialize(ref reader, extensionType, options));
                }
            }
            return data;
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var item in value)
            {
                writer.WritePropertyName(item.Key);
                JsonSerializer.Serialize(writer, item.Value, options);
            }
            writer.WriteEndObject();
        }
    }
}
