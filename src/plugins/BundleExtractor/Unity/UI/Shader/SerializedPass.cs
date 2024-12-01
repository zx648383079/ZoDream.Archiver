using System.Collections.Generic;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SerializedPass
    {
        public List<Hash128> m_EditorDataHash;
        public byte[] m_Platforms;
        public ushort[] m_LocalKeywordMask;
        public ushort[] m_GlobalKeywordMask;
        public List<KeyValuePair<string, int>> m_NameIndices;
        public PassType m_Type;
        public SerializedShaderState m_State;
        public uint m_ProgramMask;
        public SerializedProgram progVertex;
        public SerializedProgram progFragment;
        public SerializedProgram progGeometry;
        public SerializedProgram progHull;
        public SerializedProgram progDomain;
        public SerializedProgram progRayTracing;
        public bool m_HasInstancingVariant;
        public string m_UseName;
        public string m_Name;
        public string m_TextureName;
        public SerializedTagMap m_Tags;
        public ushort[] m_SerializedKeywordStateMask;

        public SerializedPass(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();

            if (version.GreaterThanOrEquals(2020, 2)) //2020.2 and up
            {
                int numEditorDataHash = reader.ReadInt32();
                m_EditorDataHash = new List<Hash128>();
                for (int i = 0; i < numEditorDataHash; i++)
                {
                    m_EditorDataHash.Add(new Hash128(reader));
                }
                reader.AlignStream();
                m_Platforms = reader.ReadArray(r => r.ReadByte());
                reader.AlignStream();
                if (version.LessThan(2021, 1)) //2021.1 and down
                {
                    m_LocalKeywordMask = reader.ReadArray(r => r.ReadUInt16());
                    reader.AlignStream();
                    m_GlobalKeywordMask = reader.ReadArray(r => r.ReadUInt16());
                    reader.AlignStream();
                }
            }

            int numIndices = reader.ReadInt32();
            m_NameIndices = new List<KeyValuePair<string, int>>();
            for (int i = 0; i < numIndices; i++)
            {
                m_NameIndices.Add(new KeyValuePair<string, int>(reader.ReadAlignedString(), reader.ReadInt32()));
            }

            m_Type = (PassType)reader.ReadInt32();
            m_State = new();
            reader.Get<IBundleElementScanner>().TryRead(reader, m_State);
            m_ProgramMask = reader.ReadUInt32();
            progVertex = new SerializedProgram(reader);
            progFragment = new SerializedProgram(reader);
            progGeometry = new SerializedProgram(reader);
            progHull = new SerializedProgram(reader);
            progDomain = new SerializedProgram(reader);
            if (version.GreaterThanOrEquals(2019, 3)) //2019.3 and up
            {
                progRayTracing = new SerializedProgram(reader);
            }
            m_HasInstancingVariant = reader.ReadBoolean();
            if (version.GreaterThanOrEquals(2018)) //2018 and up
            {
                var m_HasProceduralInstancingVariant = reader.ReadBoolean();
            }
            reader.AlignStream();
            m_UseName = reader.ReadAlignedString();
            m_Name = reader.ReadAlignedString();
            m_TextureName = reader.ReadAlignedString();
            m_Tags = new SerializedTagMap(reader);
            if (version.Major == 2021 && version.Minor >= 2) //2021.2 ~2021.x
            {
                m_SerializedKeywordStateMask = reader.ReadArray(r => r.ReadUInt16());
                reader.AlignStream();
            }
        }
    }

}
