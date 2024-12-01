using System.Collections.Generic;
using System.Numerics;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class UnityPropertySheet
    {
        public List<KeyValuePair<string, UnityTexEnv>> m_TexEnvs;
        public List<KeyValuePair<string, int>> m_Ints;
        public List<KeyValuePair<string, float>> m_Floats;
        public List<KeyValuePair<string, Vector4>> m_Colors;

        public UnityPropertySheet(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();

            int m_TexEnvsSize = reader.ReadInt32();
            m_TexEnvs = [];
            for (int i = 0; i < m_TexEnvsSize; i++)
            {
                var key = reader.ReadAlignedString();
                var value = new UnityTexEnv();
                reader.Get<IBundleElementScanner>().TryRead(reader, value);
                m_TexEnvs.Add(new(key, value));
            }

            if (version.GreaterThanOrEquals(2021, 1)) //2021.1 and up
            {
                int m_IntsSize = reader.ReadInt32();
                m_Ints = [];
                for (int i = 0; i < m_IntsSize; i++)
                {
                    m_Ints.Add(new(reader.ReadAlignedString(), reader.ReadInt32()));
                }
            }

            int m_FloatsSize = reader.ReadInt32();
            m_Floats = [];
            for (int i = 0; i < m_FloatsSize; i++)
            {
                m_Floats.Add(new(reader.ReadAlignedString(), reader.ReadSingle()));
            }

            int m_ColorsSize = reader.ReadInt32();
            m_Colors = [];
            for (int i = 0; i < m_ColorsSize; i++)
            {
                m_Colors.Add(new(reader.ReadAlignedString(), reader.ReadVector4()));
            }
        }
    }

}
