using System;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class VectorParameter : IElementLoader
    {
        public int m_NameIndex;
        public int m_Index;
        public int m_ArraySize;
        public sbyte m_Type;
        public sbyte m_Dim;

        public void Read(IBundleBinaryReader reader)
        {
            ReadBase(reader, () => { });
        }

        public void ReadBase(IBundleBinaryReader reader, Action cb)
        {
            m_NameIndex = reader.ReadInt32();
            m_Index = reader.ReadInt32();
            m_ArraySize = reader.ReadInt32();
            cb.Invoke();
            m_Type = reader.ReadSByte();
            m_Dim = reader.ReadSByte();
            reader.AlignStream();
        }
    }

}
