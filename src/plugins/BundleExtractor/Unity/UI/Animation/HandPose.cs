using System.Numerics;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class HandPose
    {
        public XForm<Vector3> m_GrabX;
        public float[] m_DoFArray;
        public float m_Override;
        public float m_CloseOpen;
        public float m_InOut;
        public float m_Grab;
        public HandPose() { }

        public HandPose(UIReader reader)
        {
            m_GrabX = reader.ReadXForm();
            m_DoFArray = reader.ReadArray(r => r.ReadSingle());
            m_Override = reader.ReadSingle();
            m_CloseOpen = reader.ReadSingle();
            m_InOut = reader.ReadSingle();
            m_Grab = reader.ReadSingle();
        }

        public static HandPose ParseGI(UIReader reader)
        {
            var handPose = new HandPose();
            handPose.m_GrabX = UIReader.Parse(reader.ReadXForm4());
            handPose.m_DoFArray = reader.ReadArray(20, r => r.ReadSingle());
            handPose.m_Override = reader.ReadSingle();
            handPose.m_CloseOpen = reader.ReadSingle();
            handPose.m_InOut = reader.ReadSingle();
            handPose.m_Grab = reader.ReadSingle();

            return handPose;
        }
    }

}
