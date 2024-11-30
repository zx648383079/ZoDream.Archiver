using System;
using System.IO;
using System.Numerics;
using System.Text;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Producers;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class UIReader(EndianReader reader,
        ObjectInfo data,
        ISerializedFile source,
        IBundleOptions options): EndianReader(reader.BaseStream, reader.EndianType, reader.IsAlignArray)
    {

        public ObjectInfo Data { get; private set; } = data;
        public ISerializedFile Source { get; private set; } = source;
        public IBundleOptions Options { get; private set; } = options;

        public ElementIDType Type
        {
            get
            {
                if (Enum.IsDefined(typeof(ElementIDType), (int)Data.ClassID))
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


        public Stream OpenResource(string fileName)
        {
            return Source.Container!.OpenResource(fileName, Source);
        }

        public Stream? OpenResource(StreamingInfo info)
        {
            if (string.IsNullOrWhiteSpace(info.path))
            {
                return null;
            }
            var stream = OpenResource(info.path);
            if (stream is null)
            {
                return null;
            }
            return new PartialStream(stream, info.offset, info.size);
        }

        public Vector4 ReadVector4()
        {
            return new Vector4(ReadSingle(), ReadSingle(),
                ReadSingle(), ReadSingle());
        }

        public Vector2 ReadVector2()
        {
            return new Vector2(ReadSingle(), ReadSingle());
        }
        public Vector3 ReadVector3()
        {
            if (Version.GreaterThanOrEquals(5, 4))
            {
                return new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
            }
            else
            {
                var res = new Vector4(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
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
                length = ReadInt32();
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
            var length = ReadInt32();
            var items = new XForm<Vector3>[length];
            for (int i = 0; i < length; i++)
            {
                items[i] = ReadXForm();
            }
            return items;
        }

        
        public Matrix4x4 ReadMatrix()
        {
            var data = new Matrix4x4();
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    data[r, c] = ReadSingle();
                }
            }
            return data;
        }

        public Matrix4x4[] ReadMatrixArray()
        {
            return reader.ReadArray(_ => ReadMatrix());
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
            return false;
        }

        internal bool IsSRCB2()
        {
            return false;
        }

        internal bool IsLoveAndDeepSpace()
        {
            if (string.IsNullOrWhiteSpace(Options.Package))
            {
                return false;
            }
            return Options.Producer == PaperProducer.ProducerName && Options.Package.Contains("deepspace");
        }

        

        internal bool IsShiningNikki()
        {
            if (string.IsNullOrWhiteSpace(Options.Package))
            {
                return false;
            }
            return Options.Producer == PaperProducer.ProducerName && Options.Package.Contains(".nn4");
        }

        internal bool IsArknightsEndfield()
        {
            if (string.IsNullOrWhiteSpace(Options.Package))
            {
                return false;
            }
            return Options.Package.Contains("endfield");
        }

        internal bool IsNaraka()
        {
            if (string.IsNullOrWhiteSpace(Options.Package))
            {
                return false;
            }
            return Options.Package.Contains("naraka");
        }

        internal bool IsBH3()
        {
            if (string.IsNullOrWhiteSpace(Options.Package))
            {
                return false;
            }
            return Options.Producer == MiHoYoProducer.ProducerName
                && Options.Package.Contains("bh3");
        }

        internal bool IsBH3Group()
        {
            return false;
        }

        internal bool IsExAstris()
        {
            if (string.IsNullOrWhiteSpace(Options.Package))
            {
                return false;
            }
            return Options.Package.Contains("yinjiao");
        }

        internal bool IsZZZCB1()
        {
            if (string.IsNullOrWhiteSpace(Options.Package))
            {
                return false;
            }
            return Options.Producer == MiHoYoProducer.ProducerName
                && Options.Package.Contains("zenless");
        }

        
        internal bool IsGICB1()
        {
            return false;
        }
        internal bool IsGIPack()
        {
            return false;
        }
        internal bool IsGI()
        {
            if (string.IsNullOrWhiteSpace(Options.Package))
            {
                return false;
            }
            return Options.Producer == MiHoYoProducer.ProducerName 
                && Options.Package.Contains("genshin");
        }
     
        internal bool IsGICB2()
        {
            return false;
        }

        internal bool IsGICB3()
        {
            return false;
        }

        internal bool IsGICB3Pre()
        {
            return false;
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
