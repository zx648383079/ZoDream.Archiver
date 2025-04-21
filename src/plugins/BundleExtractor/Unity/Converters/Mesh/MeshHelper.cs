using System;
using System.Buffers.Binary;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    public static class MeshHelper
    {
        public enum VertexChannelFormat
        {
            Float,
            Float16,
            Color,
            Byte,
            UInt32
        }

        public enum VertexFormat2017
        {
            Float,
            Float16,
            Color,
            UNorm8,
            SNorm8,
            UNorm16,
            SNorm16,
            UInt8,
            SInt8,
            UInt16,
            SInt16,
            UInt32,
            SInt32
        }

        public enum VertexFormat
        {
            Float,
            Float16,
            UNorm8,
            SNorm8,
            UNorm16,
            SNorm16,
            UInt8,
            SInt8,
            UInt16,
            SInt16,
            UInt32,
            SInt32
        }

        public static VertexFormat ToVertexFormat(int format, Version version)
        {
            if (version.LessThan(2017))
            {
                return (VertexChannelFormat)format switch
                {
                    VertexChannelFormat.Float => VertexFormat.Float,
                    VertexChannelFormat.Float16 => VertexFormat.Float16,
                    //in 4.x is size 4
                    VertexChannelFormat.Color => VertexFormat.UNorm8,
                    VertexChannelFormat.Byte => VertexFormat.UInt8,
                    //in 5.x
                    VertexChannelFormat.UInt32 => VertexFormat.UInt32,
                    _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
                };
            }
            else if (version.LessThan(2019))
            {
                return (VertexFormat2017)format switch
                {
                    VertexFormat2017.Float => VertexFormat.Float,
                    VertexFormat2017.Float16 => VertexFormat.Float16,
                    VertexFormat2017.Color or VertexFormat2017.UNorm8 => VertexFormat.UNorm8,
                    VertexFormat2017.SNorm8 => VertexFormat.SNorm8,
                    VertexFormat2017.UNorm16 => VertexFormat.UNorm16,
                    VertexFormat2017.SNorm16 => VertexFormat.SNorm16,
                    VertexFormat2017.UInt8 => VertexFormat.UInt8,
                    VertexFormat2017.SInt8 => VertexFormat.SInt8,
                    VertexFormat2017.UInt16 => VertexFormat.UInt16,
                    VertexFormat2017.SInt16 => VertexFormat.SInt16,
                    VertexFormat2017.UInt32 => VertexFormat.UInt32,
                    VertexFormat2017.SInt32 => VertexFormat.SInt32,
                    _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
                };
            }
            else
            {
                return (VertexFormat)format;
            }
        }


        public static uint GetFormatSize(VertexFormat format)
        {
            switch (format)
            {
                case VertexFormat.Float:
                case VertexFormat.UInt32:
                case VertexFormat.SInt32:
                    return 4u;
                case VertexFormat.Float16:
                case VertexFormat.UNorm16:
                case VertexFormat.SNorm16:
                case VertexFormat.UInt16:
                case VertexFormat.SInt16:
                    return 2u;
                case VertexFormat.UNorm8:
                case VertexFormat.SNorm8:
                case VertexFormat.UInt8:
                case VertexFormat.SInt8:
                    return 1u;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        public static bool IsIntFormat(VertexFormat format)
        {
            return format >= VertexFormat.UInt8;
        }

        public static float[] BytesToFloatArray(byte[] inputBytes, VertexFormat format)
        {
            var size = GetFormatSize(format);
            var len = inputBytes.Length / size;
            var result = new float[len];
            for (int i = 0; i < len; i++)
            {
                switch (format)
                {
                    case VertexFormat.Float:
                        result[i] = BinaryPrimitives.ReadSingleLittleEndian(inputBytes.AsSpan(i * 4));
                        break;
                    case VertexFormat.Float16:
                        result[i] = (float)BitConverter.ToHalf(inputBytes, i * 2);
                        break;
                    case VertexFormat.UNorm8:
                        result[i] = inputBytes[i] / 255f;
                        break;
                    case VertexFormat.SNorm8:
                        result[i] = Math.Max((sbyte)inputBytes[i] / 127f, -1f);
                        break;
                    case VertexFormat.UNorm16:
                        result[i] = BinaryPrimitives.ReadUInt16LittleEndian(inputBytes.AsSpan(i * 2)) / 65535f;
                        break;
                    case VertexFormat.SNorm16:
                        result[i] = Math.Max(BinaryPrimitives.ReadInt16LittleEndian(inputBytes.AsSpan(i * 2)) / 32767f, -1f);
                        break;
                }
            }
            return result;
        }

        public static int[] BytesToIntArray(byte[] inputBytes, VertexFormat format)
        {
            var size = GetFormatSize(format);
            var len = inputBytes.Length / size;
            var result = new int[len];
            for (int i = 0; i < len; i++)
            {
                switch (format)
                {
                    case VertexFormat.UInt8:
                    case VertexFormat.SInt8:
                        result[i] = inputBytes[i];
                        break;
                    case VertexFormat.UInt16:
                    case VertexFormat.SInt16:
                        result[i] = BinaryPrimitives.ReadInt16LittleEndian(inputBytes.AsSpan(i * 2));
                        break;
                    case VertexFormat.UInt32:
                    case VertexFormat.SInt32:
                        result[i] = BinaryPrimitives.ReadInt32LittleEndian(inputBytes.AsSpan(i * 4));
                        break;
                }
            }
            return result;
        }
    }
}
