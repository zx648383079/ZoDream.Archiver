using System;
using System.Collections.Generic;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    
    internal sealed class SpriteAtlas(UIReader reader) : NamedObject(reader)
    {
        public List<PPtr<Sprite>> m_PackedSprites;
        public Dictionary<KeyValuePair<Guid, long>, SpriteAtlasData> m_RenderDataMap;
        public bool m_IsVariant;

        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            var m_PackedSpritesSize = reader.ReadInt32();
            m_PackedSprites = [];
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
