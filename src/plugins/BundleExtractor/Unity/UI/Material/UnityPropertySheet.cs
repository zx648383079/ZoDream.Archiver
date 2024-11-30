using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class UnityPropertySheet
    {
        public List<KeyValuePair<string, UnityTexEnv>> m_TexEnvs;
        public List<KeyValuePair<string, int>> m_Ints;
        public List<KeyValuePair<string, float>> m_Floats;
        public List<KeyValuePair<string, Vector4>> m_Colors;

        public UnityPropertySheet(UIReader reader)
        {
            var version = reader.Version;

            int m_TexEnvsSize = reader.ReadInt32();
            m_TexEnvs = new List<KeyValuePair<string, UnityTexEnv>>();
            for (int i = 0; i < m_TexEnvsSize; i++)
            {
                m_TexEnvs.Add(new(reader.ReadAlignedString(), new UnityTexEnv(reader)));
            }

            if (version.GreaterThanOrEquals(2021, 1)) //2021.1 and up
            {
                int m_IntsSize = reader.ReadInt32();
                m_Ints = new List<KeyValuePair<string, int>>();
                for (int i = 0; i < m_IntsSize; i++)
                {
                    m_Ints.Add(new(reader.ReadAlignedString(), reader.ReadInt32()));
                }
            }

            int m_FloatsSize = reader.ReadInt32();
            m_Floats = new List<KeyValuePair<string, float>>();
            for (int i = 0; i < m_FloatsSize; i++)
            {
                m_Floats.Add(new(reader.ReadAlignedString(), reader.ReadSingle()));
            }

            int m_ColorsSize = reader.ReadInt32();
            m_Colors = new List<KeyValuePair<string, Vector4>>();
            for (int i = 0; i < m_ColorsSize; i++)
            {
                m_Colors.Add(new(reader.ReadAlignedString(), reader.ReadVector4()));
            }
        }
    }

}
