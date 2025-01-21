using System.Collections.Generic;
using System.Numerics;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

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

        public HumanPose(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();
            m_RootX = reader.ReadXForm();
            m_LookAtPosition = version.GreaterThanOrEquals(5, 4) ? reader.ReadVector3Or4() :
                UnityReaderExtension.Parse(reader.ReadVector4());//5.4 and up
            m_LookAtWeight = reader.ReadVector4();

            int numGoals = reader.ReadInt32();
            m_GoalArray = [];
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

    }

}
