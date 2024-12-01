using System.Numerics;
using ZoDream.Shared.Bundle;

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

        public HandPose(IBundleBinaryReader reader)
        {
            m_GrabX = reader.ReadXForm();
            m_DoFArray = reader.ReadArray(r => r.ReadSingle());
            m_Override = reader.ReadSingle();
            m_CloseOpen = reader.ReadSingle();
            m_InOut = reader.ReadSingle();
            m_Grab = reader.ReadSingle();
        }

    }

}
