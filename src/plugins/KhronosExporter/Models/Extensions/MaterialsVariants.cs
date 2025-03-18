namespace ZoDream.KhronosExporter.Models
{
    public class MaterialsVariants
    {
        public const string ExtensionName = "KHR_materials_variants";

        public object[]? Variants { get; set; }
        public object[]? Mappings { get; set; }
    }
}
