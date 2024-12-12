using System.Collections.Generic;

namespace ZoDream.Live2dExporter.Models
{
    internal class JsonModelFileReferences
    {
        public string Moc { get; set; }
        public string[] Textures { get; set; }
        public string Physics { get; set; }
        public Dictionary<string, List<JsonModelMotion>> Motions { get; set; }
    }
}
