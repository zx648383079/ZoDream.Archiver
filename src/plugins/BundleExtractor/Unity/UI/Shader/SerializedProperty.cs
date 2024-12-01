using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SerializedProperty
    {
        public string m_Name;
        public string m_Description;
        public string[] m_Attributes;
        public SerializedPropertyType m_Type;
        public SerializedPropertyFlag m_Flags;
        public float[] m_DefValue;
        public SerializedTextureProperty m_DefTexture;

        public SerializedProperty(IBundleBinaryReader reader)
        {
            m_Name = reader.ReadAlignedString();
            m_Description = reader.ReadAlignedString();
            m_Attributes = reader.ReadArray(r => r.ReadString());
            m_Type = (SerializedPropertyType)reader.ReadInt32();
            m_Flags = (SerializedPropertyFlag)reader.ReadUInt32();
            m_DefValue = reader.ReadArray(4, (r, _) => r.ReadSingle());
            m_DefTexture = new SerializedTextureProperty(reader);
        }
    }

}
