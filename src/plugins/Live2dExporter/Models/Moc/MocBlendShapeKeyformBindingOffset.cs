using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.Bundle;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocBlendShapeKeyFormBindingOffsetPtr
    {
        public uint ParameterBindingSourcesIndices { get; private set; }
        public uint KeyFormSourcesBlendShapeIndices { get; private set; }
        public uint KeyFormSourcesBlendShapeCounts { get; private set; }
        public uint BlendShapeConstraintIndexSourcesBeginIndices { get; private set; }
        public uint BlendShapeConstraintIndexSourcesCounts { get; private set; }

        public void Read(IBundleBinaryReader reader)
        {
            ParameterBindingSourcesIndices = reader.ReadUInt32();
            KeyFormSourcesBlendShapeIndices = reader.ReadUInt32();
            KeyFormSourcesBlendShapeCounts = reader.ReadUInt32();
            BlendShapeConstraintIndexSourcesBeginIndices = reader.ReadUInt32();
            BlendShapeConstraintIndexSourcesCounts = reader.ReadUInt32();
        }
    }
    internal class MocBlendShapeKeyFormBindingOffset
    {
        public int[] ParameterBindingSourcesIndices { get; private set; }
        public int[] KeyFormSourcesBlendShapeIndices { get; private set; }
        public int[] KeyFormSourcesBlendShapeCounts { get; private set; }
        public int[] BlendShapeConstraintIndexSourcesBeginIndices { get; private set; }
        public int[] BlendShapeConstraintIndexSourcesCounts { get; private set; }

        public void Read(IBundleBinaryReader reader, int count)
        {
            var ptr = new MocBlendShapeKeyFormBindingOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            ParameterBindingSourcesIndices = reader.ReadArray(ptr.ParameterBindingSourcesIndices, count, () => reader.ReadInt32());
            KeyFormSourcesBlendShapeIndices = reader.ReadArray(ptr.KeyFormSourcesBlendShapeIndices, count, () => reader.ReadInt32());
            KeyFormSourcesBlendShapeCounts = reader.ReadArray(ptr.KeyFormSourcesBlendShapeCounts, count, () => reader.ReadInt32());
            BlendShapeConstraintIndexSourcesBeginIndices = reader.ReadArray(ptr.BlendShapeConstraintIndexSourcesBeginIndices, count, () => reader.ReadInt32());
            BlendShapeConstraintIndexSourcesCounts = reader.ReadArray(ptr.BlendShapeConstraintIndexSourcesCounts, count, () => reader.ReadInt32());

            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
