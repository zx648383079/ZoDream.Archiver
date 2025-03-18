namespace ZoDream.KhronosExporter.Models
{
    public class MaterialsIridescence
    {
        public const string ExtensionName = "KHR_materials_iridescence";

        public float IridescenceFactor { get; set; }

        public TextureInfo? IridescenceTexture { get; set; }

        public float IridescenceIor { get; set; } = 1.3f;

        public float IridescenceThicknessMinimum { get; set; } = 100;

        public float IridescenceThicknessMaximum { get; set; } = 400;

        public TextureInfo? IridescenceThicknessTexture { get; set; }
    }
}
