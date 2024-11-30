using System.Collections.Generic;
using System.Numerics;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class Avatar : NamedObject
    {
        public uint m_AvatarSize;
        public AvatarConstant m_Avatar;
        public Dictionary<uint, string> m_TOS;

        public Avatar(UIReader reader) : base(reader)
        {
            m_AvatarSize = reader.ReadUInt32();
            m_Avatar = new AvatarConstant(reader);

            int numTOS = reader.ReadInt32();
            m_TOS = new Dictionary<uint, string>();
            for (int i = 0; i < numTOS; i++)
            {
                m_TOS.Add(reader.ReadUInt32(), reader.ReadAlignedString());
            }

            //HumanDescription m_HumanDescription 2019 and up
        }

        public string FindBonePath(uint hash)
        {
            m_TOS.TryGetValue(hash, out string path);
            return path;
        }
    }
}
