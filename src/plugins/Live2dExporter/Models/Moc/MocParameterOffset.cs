using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Live2dExporter.Models
{
    internal enum MocParameterType: uint
    {
        NORMAL = 0,
        BLEND_SHAPE = 1,
    }
    internal class MocParameterOffsetPtr
    {
        public uint RuntimeSpace0 { get; private set; }
        public uint Ids { get; private set; }
        public uint MaxValues { get; private set; }
        public uint MinValues { get; private set; }
        public uint DefaultValues { get; private set; }
        public uint IsRepeat { get; private set; }
        public uint DecimalPlaces { get; private set; }
        public uint ParameterBindingSourcesBeginIndices { get; private set; }
        public uint ParameterBindingSourcesCounts { get; private set; }

        public void Read(BinaryReader reader)
        {
            RuntimeSpace0 = reader.ReadUInt32();
            Ids = reader.ReadUInt32();
            MaxValues = reader.ReadUInt32();
            MinValues = reader.ReadUInt32();
            DefaultValues = reader.ReadUInt32();
            IsRepeat = reader.ReadUInt32();
            DecimalPlaces = reader.ReadUInt32();
            ParameterBindingSourcesBeginIndices = reader.ReadUInt32();
            ParameterBindingSourcesCounts = reader.ReadUInt32();
        }
    }
    internal class MocParameterOffset
    {
        public string[] Ids {  get; private set; }
        public float[] MaxValues { get; private set; }
        public float[] MinValues { get; private set; }
        public float[] DefaultValues { get; private set; }
        public bool[] IsRepeat { get; private set; }
        public uint[] DecimalPlaces { get; private set; }
        public int[] ParameterBindingSourcesBeginIndices { get; private set; }
        public int[] ParameterBindingSourcesCounts { get; private set; }

        public void Read(BinaryReader reader, int count)
        {
            var ptr = new MocParameterOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;
            Ids = reader.ReadArray(ptr.Ids, count, () => IOExtension.ConvertString(reader.ReadBytes(64)));
            MaxValues = reader.ReadArray(ptr.MaxValues, count, () => reader.ReadSingle());
            MinValues = reader.ReadArray(ptr.MinValues, count, () => reader.ReadSingle());
            DefaultValues = reader.ReadArray(ptr.DefaultValues, count, () => reader.ReadSingle());
            IsRepeat = reader.ReadArray(ptr.IsRepeat, count, () => reader.ReadUInt32() > 0);
            DecimalPlaces = reader.ReadArray(ptr.DecimalPlaces, count, () => reader.ReadUInt32());
            ParameterBindingSourcesBeginIndices = reader.ReadArray(ptr.ParameterBindingSourcesBeginIndices, count, () => reader.ReadInt32());
            ParameterBindingSourcesCounts = reader.ReadArray(ptr.ParameterBindingSourcesCounts, count, () => reader.ReadInt32());


            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }

    internal class MocParameterOffsetsV4_2Ptr
    {
        public uint ParameterTypes { get; private set; }
        public uint BlendShapeParameterBindingSourcesBeginIndices { get; private set; }
        public uint BlendShapeParameterBindingSourcesCounts { get; private set; }
        
        public void Read(BinaryReader reader)
        {
            ParameterTypes = reader.ReadUInt32();
            BlendShapeParameterBindingSourcesBeginIndices = reader.ReadUInt32();
            BlendShapeParameterBindingSourcesCounts = reader.ReadUInt32();
        }
    }

    internal class MocParameterOffsetsV4_2
    {
        public MocParameterType[] ParameterTypes { get; private set; }
        public int[] BlendShapeParameterBindingSourcesBeginIndices { get; private set; }
        public int[] BlendShapeParameterBindingSourcesCounts { get; private set; }

        public void Read(BinaryReader reader, int count)
        {
            var ptr = new MocParameterOffsetsV4_2Ptr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            ParameterTypes = reader.ReadArray(ptr.ParameterTypes, count, () => (MocParameterType)reader.ReadUInt32());
            BlendShapeParameterBindingSourcesBeginIndices = reader.ReadArray(ptr.BlendShapeParameterBindingSourcesBeginIndices, count, () => reader.ReadInt32());
            BlendShapeParameterBindingSourcesCounts = reader.ReadArray(ptr.BlendShapeParameterBindingSourcesCounts, count, () => reader.ReadInt32());

            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
