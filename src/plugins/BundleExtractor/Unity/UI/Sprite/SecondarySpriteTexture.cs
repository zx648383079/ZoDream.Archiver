using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SecondarySpriteTexture
    {
        public PPtr<Texture2D> texture;
        public string name;

        public SecondarySpriteTexture(UIReader reader)
        {
            texture = new PPtr<Texture2D>(reader);
            name = reader.ReadStringZeroTerm();
        }
    }
}
