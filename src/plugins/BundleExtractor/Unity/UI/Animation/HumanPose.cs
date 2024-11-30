using System.Collections.Generic;
using System.Numerics;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class HumanPose
    {
        public XForm<Vector3> m_RootX;
        public Vector3 m_LookAtPosition;
        public Vector4 m_LookAtWeight;
        public List<HumanGoal> m_GoalArray;
        public HandPose m_LeftHandPose;
        public HandPose m_RightHandPose;
        public float[] m_DoFArray;
        public Vector3[] m_TDoFArray;
        public HumanPose() { }

        public HumanPose(UIReader reader)
        {
            var version = reader.Version;
            m_RootX = reader.ReadXForm();
            m_LookAtPosition = version.GreaterThanOrEquals(5, 4) ? reader.ReadVector3() :
                UIReader.Parse(reader.ReadVector4());//5.4 and up
            m_LookAtWeight = reader.ReadVector4();

            int numGoals = reader.ReadInt32();
            m_GoalArray = new List<HumanGoal>();
            for (int i = 0; i < numGoals; i++)
            {
                m_GoalArray.Add(new HumanGoal(reader));
            }

            m_LeftHandPose = new HandPose(reader);
            m_RightHandPose = new HandPose(reader);

            m_DoFArray = reader.ReadArray(r => r.ReadSingle());

            if (version.GreaterThanOrEquals(5, 2))//5.2 and up
            {
                m_TDoFArray = reader.ReadVector3Array();
            }
        }

        public static HumanPose ParseGI(UIReader reader)
        {
            var version = reader.Version;
            var humanPose = new HumanPose();

            humanPose.m_RootX = UIReader.Parse(reader.ReadXForm4());
            humanPose.m_LookAtPosition = UIReader.Parse(reader.ReadVector4());
            humanPose.m_LookAtWeight = reader.ReadVector4();

            humanPose.m_GoalArray = new List<HumanGoal>();
            for (int i = 0; i < 4; i++)
            {
                humanPose.m_GoalArray.Add(HumanGoal.ParseGI(reader));
            }

            humanPose.m_LeftHandPose = HandPose.ParseGI(reader);
            humanPose.m_RightHandPose = HandPose.ParseGI(reader);

            humanPose.m_DoFArray = reader.ReadArray(0x37, r => r.ReadSingle());

            humanPose.m_TDoFArray = reader.ReadArray(0x15, _ => UIReader.Parse(reader.ReadVector4()));

            reader.Position += 4;

            return humanPose;
        }
    }

}
