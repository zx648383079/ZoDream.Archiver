using System.Collections.Specialized;
using System.IO;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.SerializedFiles;

namespace ZoDream.BundleExtractor.UI
{
    public class UIObject
    {
        
        public UIObject(UIReader reader)
        {
            _reader = reader;
            Type = _reader.Type;
            if (_reader.Platform == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.Reader.ReadUInt32();
            }
        }

        protected readonly UIReader _reader;

        public long FileID => _reader.Data.FileID;
        public ISerializedFile AssetFile => _reader.Source;
        public ElementIDType Type { get; private set; }
        public virtual string Name => string.Empty;


        public string Dump()
        {
            //if (serializedType?.m_Type != null)
            //{
            //    return TypeTreeHelper.ReadTypeString(serializedType.m_Type, reader);
            //}
            return string.Empty;
        }

        public string Dump(TypeTree m_Type)
        {
            //if (m_Type != null)
            //{
            //    return TypeTreeHelper.ReadTypeString(m_Type, reader);
            //}
            return string.Empty;
        }

        public OrderedDictionary? ToType()
        {
            //if (serializedType?.m_Type != null)
            //{
            //    return TypeTreeHelper.ReadType(serializedType.m_Type, reader);
            //}
            return null;
        }

        public OrderedDictionary? ToType(TypeTree m_Type)
        {
            //if (m_Type != null)
            //{
            //    return TypeTreeHelper.ReadType(m_Type, reader);
            //}
            return null;
        }

        public Stream GetRawData()
        {
            _reader.Position = 0;
            return _reader.Reader.BaseStream;
        }
    }
}
