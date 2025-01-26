using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.KhronosExporter.Models
{
    public class Buffer : LogicalChildOfRoot
    {
        public string Uri { get; set; }

        public int ByteLength { get; set; }

    }
}
