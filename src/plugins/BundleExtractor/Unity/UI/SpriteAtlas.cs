using System;
using System.Collections.Generic;
using System.Numerics;

namespace ZoDream.BundleExtractor.Unity.UI
{
    
    internal sealed class SpriteAtlas : NamedObject
    {
        public List<PPtr<Sprite>> m_PackedSprites;
        public Dictionary<KeyValuePair<Guid, long>, SpriteAtlasData> m_RenderDataMap;
        public bool m_IsVariant;

        public SpriteAtlas(UIReader reader) : base(reader)
        {
            var m_PackedSpritesSize = reader.ReadInt32();
            m_PackedSprites = new List<PPtr<Sprite>>();
            for (int i = 0; i < m_PackedSpritesSize; i++)
            {
                m_PackedSprites.Add(new PPtr<Sprite>(reader));
            }

            var m_PackedSpriteNamesToIndex = reader.ReadArray(r => r.ReadString());

            var m_RenderDataMapSize = reader.ReadInt32();
            m_RenderDataMap = new Dictionary<KeyValuePair<Guid, long>, SpriteAtlasData>();
            for (int i = 0; i < m_RenderDataMapSize; i++)
            {
                var first = new Guid(reader.ReadBytes(16));
                var second = reader.ReadInt64();
                var value = new SpriteAtlasData(reader);
                m_RenderDataMap.Add(new KeyValuePair<Guid, long>(first, second), value);
            }
            var m_Tag = reader.ReadAlignedString();
            m_IsVariant = reader.ReadBoolean();
            reader.AlignStream();
        }
    }
}
