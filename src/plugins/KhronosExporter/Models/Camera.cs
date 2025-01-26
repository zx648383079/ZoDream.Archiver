using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.KhronosExporter.Models
{
    public class Camera : LogicalChildOfRoot
    {
        public CameraOrthographic Orthographic { get; set; }

        public CameraPerspective Perspective { get; set; }

        public CameraType Type { get; set; }

    }
}
