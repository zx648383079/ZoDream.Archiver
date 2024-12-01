using System.Numerics;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class HumanGoal
    {
        public XForm<Vector3> m_X;
        public float m_WeightT;
        public float m_WeightR;
        public Vector3 m_HintT;
        public float m_HintWeightT;
        public HumanGoal() { }

        public HumanGoal(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();
            m_X = reader.ReadXForm();
            m_WeightT = reader.ReadSingle();
            m_WeightR = reader.ReadSingle();
            if (version.Major >= 5)//5.0 and up
            {
                m_HintT = version.GreaterThanOrEquals(5, 4) ? reader.ReadVector3() :
                    UnityReaderExtension.Parse(reader.ReadVector4());//5.4 and up
                m_HintWeightT = reader.ReadSingle();
            }
        }

    }

}
