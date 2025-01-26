using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.KhronosExporter.Models
{
    public class TextureSampler : LogicalChildOfRoot
    {
        public TextureInterpolationFilter MagFilter { get; set; }

        public TextureMipMapFilter MinFilter { get; set; }

        public TextureWrapMode WrapS { get; set; }

        public TextureWrapMode WrapT { get; set; }


    }
}
