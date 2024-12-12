namespace ZoDream.Live2dExporter.Models
{
    internal class JsonModelRoot
    {
        public int Version { get; set; }
        public JsonModelFileReferences FileReferences { get; set; }
        public JsonModelGroup[] Groups { get; set; }

    }
}
