﻿using System.Collections.Specialized;
using System.IO;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class UIObject(UIReader reader) : IElementLoader
    {
        protected readonly UIReader _reader = reader;

        public long FileID => _reader.Data.FileID;
        public ISerializedFile AssetFile => _reader.Source;

        public SerializedType SerializedType => _reader.SerializedType;
        public ElementIDType Type => _reader.Type;

        public virtual string Name => string.Empty;

        public virtual void Read(IBundleBinaryReader reader)
        {
            if (_reader.Platform == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
            }
        }


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
            return new PartialStream(_reader.BaseStream, 
                _reader.Data.ByteStart, 
                _reader.Data.ByteSize);
        }

        
    }
}
