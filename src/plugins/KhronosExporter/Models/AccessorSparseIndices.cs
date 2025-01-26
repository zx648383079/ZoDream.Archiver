using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.KhronosExporter.Models
{
    public class AccessorSparseIndices : ExtraProperties
    {

        public int BufferView {  get; set; }

        public int ByteOffset { get; set; }

        public EncodingType ComponentType { get; set; }
    }
}
