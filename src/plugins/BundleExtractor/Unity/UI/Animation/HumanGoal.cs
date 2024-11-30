using System.Numerics;

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

        public HumanGoal(UIReader reader)
        {
            var version = reader.Version;
            m_X = reader.ReadXForm();
            m_WeightT = reader.ReadSingle();
            m_WeightR = reader.ReadSingle();
            if (version.Major >= 5)//5.0 and up
            {
                m_HintT = version.GreaterThanOrEquals(5, 4) ? reader.ReadVector3() :
                    UIReader.Parse(reader.ReadVector4());//5.4 and up
                m_HintWeightT = reader.ReadSingle();
            }
        }

        public static HumanGoal ParseGI(UIReader reader)
        {
            var humanGoal = new HumanGoal();

            humanGoal.m_X = UIReader.Parse(reader.ReadXForm4());
            humanGoal.m_WeightT = reader.ReadSingle();
            humanGoal.m_WeightR = reader.ReadSingle();

            humanGoal.m_HintT = UIReader.Parse(reader.ReadVector4());
            humanGoal.m_HintWeightT = reader.ReadSingle();

            var m_HintR = UIReader.Parse(reader.ReadVector4());
            var m_HintWeightR = reader.ReadSingle();

            return humanGoal;
        }
    }

}
