namespace ZoDream.KhronosExporter.Models
{
    /// <summary>
    /// 用来控制折射率
    /// </summary>
    public class MaterialsIOR
    {
        public const string ExtensionName = "KHR_materials_ior";

        public float Ior { get; set; } = 1.5f;
    }
}
