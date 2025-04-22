using System.Numerics;
using UnityEngine;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Numerics;

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

        public static Vector3 ReadVector3Or4(this IBundleBinaryReader reader)
        {
            var res = reader.ReadVector3();
            if (reader.Get<Version>().LessThan(5, 4))
            {
                reader.ReadSingle();
            }
            return res;
        }

        public static Transform<Vector3> ReadXForm(this IBundleBinaryReader reader)
        {
            var t = ReadVector3Or4(reader);
            var q = reader.ReadQuaternion();
            var s = ReadVector3Or4(reader);
            return new(t, q, s);
        }

        public static Transform<Vector3> ReadXForm4(this IBundleBinaryReader reader)
        {
            var t = reader.ReadVector4();
            var q = reader.ReadQuaternion();
            var s = reader.ReadVector4();

            return new(t.AsVector3(), q, s.AsVector3());
        }

        public static Vector3[] ReadVector3Array(this IBundleBinaryReader reader, int length = 0)
        {
            if (length == 0)
            {
                length = reader.ReadInt32();
            }
            return reader.ReadArray(length, _ => ReadVector3Or4(reader));
        }

        public static Transform<Vector3>[] ReadXFormArray(this IBundleBinaryReader reader)
        {
            return reader.ReadArray(_ => ReadXForm(reader));
        }


        public static Matrix4x4[] ReadMatrixArray(this IBundleBinaryReader reader)
        {
            return reader.ReadArray(_ => reader.ReadMatrix());
        }

        public static IPPtr<T> ReadPPtr<T>(this IBundleBinaryReader reader, IBundleSerializer serializer)
            where T : Object
        {
            return new ObjectPPtr<T>(reader.Get<ISerializedFile>(), serializer.Deserialize<PPtr>(reader));
        }

        public static IPPtr<T>[] ReadPPtrArray<T>(this IBundleBinaryReader reader, IBundleSerializer serializer)
            where T : Object
        {
            return reader.ReadArray(_ => ReadPPtr<T>(reader, serializer));
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

        public static bool IsStandalone(this BuildTarget _this)
        {
            return _this switch
            {
                BuildTarget.StandaloneWinPlayer or BuildTarget.StandaloneWin64Player or BuildTarget.StandaloneLinux or BuildTarget.StandaloneLinux64 or BuildTarget.StandaloneLinuxUniversal or BuildTarget.StandaloneOSXIntel or BuildTarget.StandaloneOSXIntel64 or BuildTarget.StandaloneOSXPPC or BuildTarget.StandaloneOSXUniversal => true,
                _ => false,
            };
        }
    }
}
