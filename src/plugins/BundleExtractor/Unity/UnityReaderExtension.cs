using System.Numerics;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity
{
    internal static class UnityReaderExtension
    {
        /// <summary>
        /// 一个字节分两半
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static byte[] To4bArray(this byte[] source) => To4bArray(source, 0, source.Length);
        /// <summary>
        /// 一个字节分两半，
        /// </summary>
        /// <param name="source"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static byte[] To4bArray(this byte[] source, int offset, int size)
        {
            var buffer = new byte[size * 2];
            for (var i = 0; i < size; i++)
            {
                var idx = i * 2;
                buffer[idx] = (byte)(source[offset + i] >> 4);
                buffer[idx + 1] = (byte)(source[offset + i] & 0xF);
            }
            return buffer;
        }
        /// <summary>
        /// 从 4b 合成 8b 一个字节
        /// </summary>
        /// <param name="source"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static byte[] From4bToArray(this byte[] source, int offset, int size)
        {
            var buffer = new byte[size / 2];
            for (var i = 0; i < size; i++)
            {
                var idx = i / 2;
                if (i % 2 == 0)
                {
                    buffer[idx] = (byte)(source[offset + i] << 4);
                }
                else
                {
                    buffer[idx] |= source[offset + i];
                }
            }
            return buffer;
        }

        public static int[] ReadInt32Array(this IBundleBinaryReader reader)
        {
            return reader.ReadArray(reader.ReadInt32);
        }
        public static Vector4 ReadVector4(this IBundleBinaryReader reader)
        {
            return new Vector4(reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle(), reader.ReadSingle());
        }

        public static Quaternion ReadQuaternion(this IBundleBinaryReader reader)
        {
            return new Quaternion(reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector2 ReadVector2(this IBundleBinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector3 ReadVector3(this IBundleBinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle());
        }
        public static Vector3 ReadVector3Or4(this IBundleBinaryReader reader)
        {
            if (reader.Get<UnityVersion>().GreaterThanOrEquals(5, 4))
            {
                return ReadVector3(reader);
            }
            else
            {
                var res = new Vector4(reader.ReadSingle(), 
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                return new(res.X, res.Y, res.Z);
                //return new Vector4(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle());
            }
        }

        public static XForm<Vector3> ReadXForm(this IBundleBinaryReader reader)
        {
            var t = ReadVector3Or4(reader);
            var q = ReadQuaternion(reader);
            var s = ReadVector3Or4(reader);

            return new XForm<Vector3>(t, q, s);
        }

        public static XForm<Vector4> ReadXForm4(this IBundleBinaryReader reader)
        {
            var t = ReadVector4(reader);
            var q = ReadQuaternion(reader);
            var s = ReadVector4(reader);

            return new XForm<Vector4>(t, q, s);
        }

        public static Vector3[] ReadVector3Array(this IBundleBinaryReader reader, int length = 0)
        {
            if (length == 0)
            {
                length = reader.ReadInt32();
            }
            var items = new Vector3[length];
            for (int i = 0; i < length; i++)
            {
                items[i] = ReadVector3Or4(reader);
            }
            return items;
        }

        public static XForm<Vector3>[] ReadXFormArray(this IBundleBinaryReader reader)
        {
            var length = reader.ReadInt32();
            var items = new XForm<Vector3>[length];
            for (int i = 0; i < length; i++)
            {
                items[i] = ReadXForm(reader);
            }
            return items;
        }


        public static Matrix4x4 ReadMatrix(this IBundleBinaryReader reader)
        {
            var data = new Matrix4x4();
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    data[r, c] = reader.ReadSingle();
                }
            }
            return data;
        }

        public static Matrix4x4[] ReadMatrixArray(this IBundleBinaryReader reader)
        {
            return reader.ReadArray(_ => ReadMatrix(reader));
        }

        public static Vector3 Parse(Vector4 data)
        {
            return new(data.X, data.Y, data.Z);
        }

        public static XForm<Vector3> Parse(XForm<Vector4> data)
        {
            return new(Parse(data.T), data.Q, Parse(data.S));
        }

        public static Matrix4x4 CreateMatrix(float[] buffer)
        {
            var data = new Matrix4x4();
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    data[r, c] = buffer[r * 4 + c];
                }
            }
            return data;
        }
        /// <summary>
        /// 自动创建元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scanner"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T CreateElement<T>(this IBundleElementScanner scanner, IBundleBinaryReader reader)
            where T : class, new()
        {
            var instance = new T();
            scanner.TryRead(reader, instance);
            return instance;
        }

        /// <summary>
        /// 自动创建元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T CreateElement<T>(this IBundleBinaryReader reader)
            where T : class, new()
        {
            var scanner = reader.Get<IBundleElementScanner>();
            return CreateElement<T>(scanner, reader);
        }

        public static BundleCodecType ToCodec(this UnityCompressionType type)
        {
            return type switch
            {
                UnityCompressionType.Lzma => BundleCodecType.Lzma,
                UnityCompressionType.Lz4 => BundleCodecType.Lz4,
                UnityCompressionType.Lz4HC => BundleCodecType.Lz4HC,
                UnityCompressionType.Lzham => BundleCodecType.Lzham,
                UnityCompressionType.Lz4Mr0k => BundleCodecType.Lz4Mr0k,
                _ => BundleCodecType.Unknown
            };
        }
    }
}
