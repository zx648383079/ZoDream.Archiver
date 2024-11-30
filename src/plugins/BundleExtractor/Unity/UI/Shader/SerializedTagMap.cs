using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SerializedTagMap
    {
        public List<KeyValuePair<string, string>> tags;

        public SerializedTagMap(UIReader reader)
        {
            int numTags = reader.ReadInt32();
            tags = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < numTags; i++)
            {
                tags.Add(new KeyValuePair<string, string>(reader.ReadAlignedString(), reader.ReadAlignedString()));
            }
        }
    }

}
