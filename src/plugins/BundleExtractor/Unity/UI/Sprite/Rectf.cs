using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Rectf
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public Rectf(BinaryReader reader)
        {
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            width = reader.ReadSingle();
            height = reader.ReadSingle();
        }
    }

}
