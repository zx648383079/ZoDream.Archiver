using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SerializedProgram
    {
        public List<SerializedSubProgram> m_SubPrograms;
        public List<List<SerializedPlayerSubProgram>> m_PlayerSubPrograms;
        public uint[][] m_ParameterBlobIndices;
        public SerializedProgramParameters m_CommonParameters;
        public ushort[] m_SerializedKeywordStateMask;

        public SerializedProgram(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();
            var scanner = reader.Get<IBundleElementScanner>();
            int numSubPrograms = reader.ReadInt32();
            m_SubPrograms = [];
            for (int i = 0; i < numSubPrograms; i++)
            {
                var sub = new SerializedSubProgram();
                scanner.TryRead(reader, sub);
                m_SubPrograms.Add(sub);
            }

            if (version.GreaterThanOrEquals(2021, 3, 10, UnityVersionType.Final, 1) || //2021.3.10f1 and up
               version.GreaterThanOrEquals(2022, 1, 13, UnityVersionType.Final, 1)) //2022.1.13f1 and up
            {
                int numPlayerSubPrograms = reader.ReadInt32();
                m_PlayerSubPrograms = new List<List<SerializedPlayerSubProgram>>();
                for (int i = 0; i < numPlayerSubPrograms; i++)
                {
                    m_PlayerSubPrograms.Add(new List<SerializedPlayerSubProgram>());
                    int numPlatformPrograms = reader.ReadInt32();
                    for (int j = 0; j < numPlatformPrograms; j++)
                    {
                        m_PlayerSubPrograms[i].Add(new SerializedPlayerSubProgram(reader));
                    }
                }

                m_ParameterBlobIndices = reader.Read2DArray((r, _, _) => r.ReadUInt32());
            }

            if (version.GreaterThanOrEquals(2020, 3, 2, UnityVersionType.Final, 1) || //2020.3.2f1 and up
               version.GreaterThanOrEquals(2021, 1, 1, UnityVersionType.Final, 1)) //2021.1.1f1 and up
            {
                m_CommonParameters = new SerializedProgramParameters(reader);
            }

            if (version.GreaterThanOrEquals(2022, 1)) //2022.1 and up
            {
                m_SerializedKeywordStateMask = reader.ReadArray(r => r.ReadUInt16());
                reader.AlignStream();
            }
        }
    }

}
