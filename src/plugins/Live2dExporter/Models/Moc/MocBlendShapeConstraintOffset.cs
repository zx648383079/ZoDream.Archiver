using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.Bundle;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocBlendShapeConstraintOffsetPtr
    {
        public uint ParameterIndices { get; private set; }
        public uint BlendShapeConstraintValueSourcesBeginIndices { get; private set; }
        public uint BlendShapeConstraintValueSourcesCounts { get; private set; }
        
        public void Read(IBundleBinaryReader reader)
        {
            ParameterIndices = reader.ReadUInt32();
            BlendShapeConstraintValueSourcesBeginIndices = reader.ReadUInt32();
            BlendShapeConstraintValueSourcesCounts = reader.ReadUInt32();
        }
    }
    internal class MocBlendShapeConstraintOffset
    {
        public int[] ParameterIndices { get; private set; }
        public int[] BlendShapeConstraintValueSourcesBeginIndices { get; private set; }
        public int[] BlendShapeConstraintValueSourcesCounts { get; private set; }

        public void Read(IBundleBinaryReader reader, int count)
        {
            var ptr = new MocBlendShapeConstraintOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            ParameterIndices = reader.ReadArray(ptr.ParameterIndices, count, () => reader.ReadInt32());
            BlendShapeConstraintValueSourcesBeginIndices = reader.ReadArray(ptr.BlendShapeConstraintValueSourcesBeginIndices, count, () => reader.ReadInt32());
            BlendShapeConstraintValueSourcesCounts = reader.ReadArray(ptr.BlendShapeConstraintValueSourcesCounts, count, () => reader.ReadInt32());

            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
