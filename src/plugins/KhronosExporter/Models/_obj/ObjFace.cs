using System.Collections.Generic;

namespace ZoDream.KhronosExporter.Models
{
    internal class ObjFace
    {
        public string MatName { get; }

        public List<ObjFaceTriangle> Triangles { get; set; } = [];

        public ObjFace(string materialName = "default")
        {
            MatName = materialName;
        }
    }
}
