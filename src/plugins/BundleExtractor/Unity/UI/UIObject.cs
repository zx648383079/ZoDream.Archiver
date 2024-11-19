using System.Collections.Specialized;
using System.IO;
using ZoDream.BundleExtractor.Unity.SerializedFiles;

namespace ZoDream.BundleExtractor.Unity.UI
{
    public class UIObject
    {

        public UIObject(UIReader reader)
        {
            _reader = reader;
        }

        public UIObject(UIReader reader, bool isReadable)
            : this(reader)
        {
            if (isReadable && _reader.Platform == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
            }
        }

        protected readonly UIReader _reader;

        public long FileID => _reader.Data.FileID;
        public ISerializedFile AssetFile => _reader.Source;

        public SerializedType SerializedType => _reader.SerializedType;
        public ElementIDType Type => _reader.Type;

        public virtual string Name => string.Empty;


        public string Dump()
        {
            if (SerializedType.OldType != null)
            {
                return TypeTreeHelper.ReadTypeString(SerializedType.OldType, _reader);
            }
            return string.Empty;
        }

        public string Dump(TypeTree m_Type)
        {
            if (m_Type != null)
            {
                return TypeTreeHelper.ReadTypeString(m_Type, _reader);
            }
            return string.Empty;
        }

        public OrderedDictionary? ToType()
        {
            if (SerializedType.OldType != null)
            {
                return TypeTreeHelper.ReadType(SerializedType.OldType, _reader);
            }
            return null;
        }

        public OrderedDictionary? ToType(TypeTree m_Type)
        {
            if (m_Type != null)
            {
                return TypeTreeHelper.ReadType(m_Type, _reader);
            }
            return null;
        }

        public Stream GetRawData()
        {
            _reader.BaseStream.Position = 0;
            return _reader.BaseStream;
        }
    }
}
