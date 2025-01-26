using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.KhronosExporter.Models
{
    public class BufferView : LogicalChildOfRoot
    {
        public int Buffer {  get; set; }

        public int ByteOffset { get; set; }

        public int ByteLength { get; set; }

        public int ByteStride { get; set; }

        public BufferMode Target { get; set; }

    }
}
