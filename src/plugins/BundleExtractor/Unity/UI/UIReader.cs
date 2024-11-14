using System;
using System.IO;
using System.Numerics;
using System.Text;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.UI
{
    public record UIReader(EndianReader Reader,
        ObjectInfo Data,
        ISerializedFile Source,
        IBundleOptions Options)
    {

        public ElementIDType Type
        {
            get
            {
                if (Enum.IsDefined(typeof(ElementIDType), Data.ClassID))
                {
                    return (ElementIDType)Data.ClassID;
                }
                else
                {
                    return ElementIDType.UnknownType;
                }
            }
        }

        public string FullPath => Source.FullPath;

        public SerializedType SerializedType => Source.TypeItems[Data.SerializedTypeIndex];
        public UnityVersion Version => Source.UnityVersion;
        public BuildTarget Platform => Source.Platform;

        public long Position
        {
            get => Reader.BaseStream.Position;
            set => Reader.BaseStream.Position = value;
        }
        public long Length => Reader.BaseStream.Length;
        public long Remaining => Length - Reader.BaseStream.Position;

        public Vector4 ReadVector4()
        {
            return new Vector4(Reader.ReadSingle(), Reader.ReadSingle(),
                Reader.ReadSingle(), Reader.ReadSingle());
        }

        public Vector2 ReadVector2()
        {
            return new Vector2(Reader.ReadSingle(), Reader.ReadSingle());
        }
        public Vector3 ReadVector3()
        {
            if (Version.Major > 5 || Version.Major == 5 && Version.Minor >= 4)
            {
                return new Vector3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle());
            }
            else
            {
                var res = new Vector4(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle());
                return new(res.X, res.Y, res.Z);
                //return new Vector4(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle());
            }
        }

        public XForm<Vector3> ReadXForm()
        {
            var t = ReadVector3();
            var q = ReadVector4();
            var s = ReadVector3();

            return new XForm<Vector3>(t, q, s);
        }

        public XForm<Vector4> ReadXForm4()
        {
            var t = ReadVector4();
            var q = ReadVector4();
            var s = ReadVector4();

            return new XForm<Vector4>(t, q, s);
        }

        public Vector3[] ReadVector3Array(int length = 0)
        {
            if (length == 0)
            {
                length = Reader.ReadInt32();
            }
            var items = new Vector3[length];
            for (int i = 0; i < length; i++)
            {
                items[i] = ReadVector3();
            }
            return items;
        }

        public XForm<Vector3>[] ReadXFormArray()
        {
            var length = Reader.ReadInt32();
            var items = new XForm<Vector3>[length];
            for (int i = 0; i < length; i++)
            {
                items[i] = ReadXForm();
            }
            return items;
        }


        public Stream ReadAsStrem(long length = -1)
        {
            if (length < 0)
            {
                length = Reader.ReadInt32();
            }
            return new PartialStream(Reader.BaseStream, length);
        }

        public T[] ReadArray<T>(Func<EndianReader, T> cb)
        {
            return Reader.ReadArray(cb);
        }
        public T[] ReadArray<T>(int length, Func<EndianReader, T> cb)
        {
            var items = new T[length];
            for (int i = 0; i < length; i++)
            {
                items[i] = cb.Invoke(Reader);
            }
            return items;
        }
        public string ReadAlignedString()
        {
            var result = "";
            var length = Reader.ReadInt32();
            if (length > 0 && length <= Remaining)
            {
                var stringData = Reader.ReadBytes(length);
                result = Encoding.UTF8.GetString(stringData);
            }
            Reader.AlignStream();
            return result;
        }
        public Matrix4x4 ReadMatrix()
        {
            var data = new Matrix4x4();
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    data[r, c] = Reader.ReadSingle();
                }
            }
            return data;
        }

        public Matrix4x4[] ReadMatrixArray()
        {
            return Reader.ReadArray(_ => ReadMatrix());
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


        #region GameType
        

        internal bool IsSR()
        {
            throw new NotImplementedException();
        }

        internal bool IsLoveAndDeepspace()
        {
            throw new NotImplementedException();
        }

        internal bool IsArknightsEndfield()
        {
            throw new NotImplementedException();
        }

        internal bool IsShiningNikki()
        {
            throw new NotImplementedException();
        }

        internal bool IsNaraka()
        {
            throw new NotImplementedException();
        }

        internal bool IsBH3()
        {
            throw new NotImplementedException();
        }

        internal bool IsExAstris()
        {
            throw new NotImplementedException();
        }

        internal bool IsZZZCB1()
        {
            throw new NotImplementedException();
        }

        
        internal bool IsGICB1()
        {
            throw new NotImplementedException();
        }
        internal bool IsGIPack()
        {
            throw new NotImplementedException();
        }
        internal bool IsGI()
        {
            throw new NotImplementedException();
        }
        internal bool IsBH3Group()
        {
            throw new NotImplementedException();
        }
        internal bool IsGICB2()
        {
            throw new NotImplementedException();
        }

        internal bool IsGICB3()
        {
            throw new NotImplementedException();
        }

        internal bool IsGICB3Pre()
        {
            throw new NotImplementedException();
        }

        internal bool IsSRCB2()
        {
            throw new NotImplementedException();
        }

        internal bool IsGISubGroup()
        {
            return IsGI() || IsGICB2() || IsGICB3() || IsGICB3Pre();
        }

        internal bool IsGIGroup()
        {
            return IsGI() || IsGIPack() || IsGICB1() || IsGICB2() || IsGICB3()
                || IsGICB3Pre();
        }

        internal bool IsSRGroup()
        {
            return IsSRCB2() || IsSR();
        }
        #endregion

    }
}
