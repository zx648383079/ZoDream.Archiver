using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SerializedTextureProperty
    {
        public string m_DefaultName;
        public TextureDimension m_TexDim;

        public SerializedTextureProperty(UIReader reader)
        {
            m_DefaultName = reader.ReadAlignedString();
            m_TexDim = (TextureDimension)reader.ReadInt32();
        }
    }

}
