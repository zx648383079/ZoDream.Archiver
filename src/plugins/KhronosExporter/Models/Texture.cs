using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.KhronosExporter.Models
{
    public class Texture : LogicalChildOfRoot
    {
        public int Sampler {  get; set; }

        public int Source {  get; set; }

    }
}
