using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.Bundle;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocBlendShapeConstraintIndicesOffset
    {
        public int[] BlendShapeConstraintSourcesIndices { get; private set; }

        public void Read(IBundleBinaryReader reader, int count)
        {
            var ptr = reader.ReadUInt32();
            var pos = reader.BaseStream.Position;

            BlendShapeConstraintSourcesIndices = reader.ReadArray(ptr, count, () => reader.ReadInt32());

            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
