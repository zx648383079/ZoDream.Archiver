using System.Text.Json.Serialization;

namespace ZoDream.KhronosExporter.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter<AlphaMode>))]
    public enum AlphaMode
    {
        /// <summary>
        /// Alpha 值将被忽略，并且渲染的输出是完全不透明的。
        /// </summary>
        OPAQUE,
        /// <summary>
        /// 渲染的输出是完全不透明的或完全透明的，具体取决于 Alpha 值和指定的 Alpha 截止值。
        /// </summary>
        MASK,
        /// <summary>
        /// 使用 alpha 值来确定渲染输出的透明度。 Alpha 截止值将被忽略。
        /// </summary>
        BLEND,
    }
}
