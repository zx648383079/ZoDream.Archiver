using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class ConstantClip
    {
        public float[] data;
        public ConstantClip() { }

        public ConstantClip(UIReader reader)
        {
            data = reader.ReadArray(r => r.ReadSingle());
        }
        public static ConstantClip ParseGI(UIReader reader)
        {
            var constantClipCount = (int)reader.ReadUInt64();
            var constantClipOffset = reader.Position + reader.ReadInt64();
            if (constantClipOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }

            var pos = reader.Position;
            reader.Position = constantClipOffset;

            var constantClip = new ConstantClip();
            constantClip.data = reader.ReadArray(constantClipCount, r => r.ReadSingle());

            reader.Position = pos;

            return constantClip;
        }
    }
}
