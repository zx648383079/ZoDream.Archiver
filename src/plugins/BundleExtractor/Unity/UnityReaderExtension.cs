using System.Numerics;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity
{
    internal static class UnityReaderExtension
    {
        public static int[] ReadInt32Array(this IBundleBinaryReader reader)
        {
            return reader.ReadArray(reader.ReadInt32);
        }
        public static Vector4 ReadVector4(this IBundleBinaryReader reader)
        {
            return new Vector4(reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector2 ReadVector2(this IBundleBinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }
        public static Vector3 ReadVector3(this IBundleBinaryReader reader)
        {
            if (reader.Get<UnityVersion>().GreaterThanOrEquals(5, 4))
            {
                return new Vector3(reader.ReadSingle(), 
                    reader.ReadSingle(), reader.ReadSingle());
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
            var t = ReadVector3(reader);
            var q = ReadVector4(reader);
            var s = ReadVector3(reader);

            return new XForm<Vector3>(t, q, s);
        }

        public static XForm<Vector4> ReadXForm4(this IBundleBinaryReader reader)
        {
            var t = ReadVector4(reader);
            var q = ReadVector4(reader);
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
                items[i] = ReadVector3(reader);
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
    }
}
