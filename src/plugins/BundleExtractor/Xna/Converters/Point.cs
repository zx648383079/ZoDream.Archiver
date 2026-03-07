using System;
using System.Numerics;
using UnityEngine;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Numerics;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class PointConverter : BundleConverter<Vector2Int>
    {
        public override Vector2Int Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return XnbReader.ReadVector2I(reader);
        }
    }

    internal class RectangleConverter : BundleConverter<Vector4Int>
    {
        public override Vector4Int Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return XnbReader.ReadVector4I(reader);
        }
    }

    internal class Vector2Converter : BundleConverter<Vector2>
    {
        public override Vector2 Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadVector2();
        }
    }

    internal class Vector3Converter : BundleConverter<Vector3>
    {
        public override Vector3 Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadVector3();
        }
    }

    internal class Vector4Converter : BundleConverter<Vector4>
    {
        public override Vector4 Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadVector4();
        }
    }

    internal class BoolConverter : BundleConverter<bool>
    {
        public override bool Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadBoolean();
        }
    }

    internal class CharConverter : BundleConverter<char>
    {
        public override char Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return XnbReader.ReadChar(reader);
        }
    }

    internal class StringConverter : BundleConverter<string>
    {
        public override string Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return XnbReader.ReadString(reader);
        }
    }

    internal class BoundingBoxConverter : BundleConverter<MinMaxAABB>
    {
        public override MinMaxAABB Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new MinMaxAABB()
            {
                Min = reader.ReadVector3(),
                Max = reader.ReadVector3(),
            };
        }
    }

    internal class ColorConverter : BundleConverter<Color>
    {
        public override Color Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new Color(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        }
    }

    internal class MatrixConverter : BundleConverter<Matrix4x4>
    {
        public override Matrix4x4 Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadMatrix();
        }
    }

    internal class DateTimeConverter : BundleConverter<DateTime>
    {
        public override DateTime Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var num = reader.ReadUInt64();
            var num2 = 13835058055282163712uL;
            var ticks = (long)(num & ~num2);
            var kind = (DateTimeKind)((num >> 62) & 3);
            return new DateTime(ticks, kind);
        }
    }

    internal class DecimalConverter : BundleConverter<decimal>
    {
        public override decimal Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadDecimal();
        }
    }

    internal class DoubleConverter : BundleConverter<double>
    {
        public override double Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadDouble();
        }
    }

    internal class ByteConverter : BundleConverter<byte>
    {
        public override byte Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadByte();
        }
    }

    internal class Int16Converter : BundleConverter<short>
    {
        public override short Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadInt16();
        }
    }

    internal class Int32Converter : BundleConverter<int>
    {
        public override int Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadInt32();
        }
    }

    internal class Int64Converter : BundleConverter<long>
    {
        public override long Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadInt64();
        }
    }

    internal class SByteConverter : BundleConverter<sbyte>
    {
        public override sbyte Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadSByte();
        }
    }

    internal class UInt16Converter : BundleConverter<ushort>
    {
        public override ushort Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadUInt16();
        }
    }

    internal class UInt32Converter : BundleConverter<uint>
    {
        public override uint Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadUInt32();
        }
    }

    internal class UInt64Converter : BundleConverter<ulong>
    {
        public override ulong Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadUInt64();
        }
    }
}
