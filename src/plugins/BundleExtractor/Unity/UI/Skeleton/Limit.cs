using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Limit
    {
        public object m_Min;
        public object m_Max;

        public Limit(UIReader reader)
        {
            var version = reader.Version;
            if (version.GreaterThanOrEquals(5, 4))//5.4 and up
            {
                m_Min = reader.ReadVector3();
                m_Max = reader.ReadVector3();
            }
            else
            {
                m_Min = reader.ReadVector4();
                m_Max = reader.ReadVector4();
            }
        }
    }

}
