using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
