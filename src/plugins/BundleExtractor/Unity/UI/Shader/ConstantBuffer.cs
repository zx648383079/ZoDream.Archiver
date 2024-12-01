using System.Collections.Generic;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class ConstantBuffer
    {
        public int m_NameIndex;
        public List<MatrixParameter> m_MatrixParams;
        public List<VectorParameter> m_VectorParams;
        public List<StructParameter> m_StructParams;
        public int m_Size;
        public bool m_IsPartialCB;

        public ConstantBuffer(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();

            m_NameIndex = reader.ReadInt32();

            int numMatrixParams = reader.ReadInt32();
            m_MatrixParams = new List<MatrixParameter>();
            for (int i = 0; i < numMatrixParams; i++)
            {
                m_MatrixParams.Add(new MatrixParameter(reader));
            }

            int numVectorParams = reader.ReadInt32();
            m_VectorParams = [];
            for (int i = 0; i < numVectorParams; i++)
            {
                m_VectorParams.Add(new VectorParameter(reader));
            }
            if (version.GreaterThanOrEquals(2017, 3)) //2017.3 and up
            {
                int numStructParams = reader.ReadInt32();
                m_StructParams = [];
                for (int i = 0; i < numStructParams; i++)
                {
                    m_StructParams.Add(new StructParameter(reader));
                }
            }
            m_Size = reader.ReadInt32();

            if (version.GreaterThanOrEquals(2020, 3, 2, UnityVersionType.Final, 1) || //2020.3.2f1 and up
              version.GreaterThanOrEquals(2021, 1, 4, UnityVersionType.Final, 1)) //2021.1.4f1 and up
            {
                m_IsPartialCB = reader.ReadBoolean();
                reader.AlignStream();
            }
        }
    }

}
