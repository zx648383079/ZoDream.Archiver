namespace ZoDream.KhronosExporter.Models
{
    public class MaterialsTransmission
    {
        public const string ExtensionName = "KHR_materials_transmission";

        public float TransmissionFactor { get; set; } = 0;

        public TextureInfo? TransmissionTexture { get; set; }
    }
}
