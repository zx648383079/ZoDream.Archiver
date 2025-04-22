using System;
using System.Collections.Generic;

namespace UnityEngine
{
    public class SpriteAtlas : Object
    {
        public IPPtr<Sprite>[] PackedSprites;
        public Dictionary<KeyValuePair<Guid, long>, SpriteAtlasData> RenderDataMap;
        public bool IsVariant;
    }
}
