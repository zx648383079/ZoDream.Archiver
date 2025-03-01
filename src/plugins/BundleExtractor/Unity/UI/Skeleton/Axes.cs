using System.Numerics;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Axes
    {
        public Vector4 m_PreQ;
        public Vector4 m_PostQ;
        public object m_Sgn;
        public Limit m_Limit;
        public float m_Length;
        public uint m_Type;

        public Axes(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();
            m_PreQ = reader.ReadVector4();
            m_PostQ = reader.ReadVector4();
            if (version.GreaterThanOrEquals(5, 4)) //5.4 and up
            {
                m_Sgn = reader.ReadVector3Or4();
            }
            else
            {
                m_Sgn = reader.ReadVector4();
            }
            m_Limit = new Limit(reader);
            m_Length = reader.ReadSingle();
            m_Type = reader.ReadUInt32();
        }
    }

}
