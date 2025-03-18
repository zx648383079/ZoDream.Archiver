namespace ZoDream.KhronosExporter.Models
{
    /// <summary>
    /// 附加在 AnimationChannelTarget
    /// </summary>
    public class AnimationPointer
    {
        public const string ExtensionName = "KHR_animation_pointer";

        public string Pointer { get; set; }
    }
}
