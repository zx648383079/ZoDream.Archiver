using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.KhronosExporter.Models
{
    public class Accessor : LogicalChildOfRoot
    {

        public int BufferView {  get; set; }

        public int ByteOffset { get; set; }

        public EncodingType ComponentType { get; set; }

        public bool Normalized { get; set; }

        public int Count { get; set; }

        public string Type { get; set; }

        public float[] Max {  get; set; }

        public float[] Min { get; set; }

        public AccessorSparse Sparse { get; set; }

    }
}
