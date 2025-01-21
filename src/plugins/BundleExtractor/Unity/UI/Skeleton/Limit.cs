using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Limit
    {
        public object m_Min;
        public object m_Max;

        public Limit(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();
            if (version.GreaterThanOrEquals(5, 4))//5.4 and up
            {
                m_Min = reader.ReadVector3Or4();
                m_Max = reader.ReadVector3Or4();
            }
            else
            {
                m_Min = reader.ReadVector4();
                m_Max = reader.ReadVector4();
            }
        }
    }

}
