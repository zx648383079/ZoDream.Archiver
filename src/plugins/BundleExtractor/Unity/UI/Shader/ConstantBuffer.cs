using System;
using System.Collections.Generic;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class ConstantBuffer : IElementLoader
    {
        public int m_NameIndex;
        public List<MatrixParameter> m_MatrixParams;
        public List<VectorParameter> m_VectorParams;
        public List<StructParameter> m_StructParams;
        public int m_Size;
        public bool m_IsPartialCB;

        public void Read(IBundleBinaryReader reader)
        {
            ReadBase(reader, () => { });
        }

        public void ReadBase(IBundleBinaryReader reader, Action cb)
        {
            var version = reader.Get<UnityVersion>();

            m_NameIndex = reader.ReadInt32();

            int numMatrixParams = reader.ReadInt32();
            m_MatrixParams = [];
            var scanner = reader.Get<IBundleElementScanner>();
            for (int i = 0; i < numMatrixParams; i++)
            {
                var instance = new MatrixParameter();
                scanner.TryRead(reader, instance);
                m_MatrixParams.Add(instance);
            }

            int numVectorParams = reader.ReadInt32();
            m_VectorParams = [];
            for (int i = 0; i < numVectorParams; i++)
            {
                var instance = new VectorParameter();
                scanner.TryRead(reader, instance);
                m_VectorParams.Add(instance);
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
                cb.Invoke();
                m_IsPartialCB = reader.ReadBoolean();
                reader.AlignStream();
            }
        }
    }

}
