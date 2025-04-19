using System;
using System.Collections.Generic;
using System.Numerics;

namespace UnityEngine
{
    public sealed class Sprite : Object
    {
        public Vector4 Rect;
        public Vector2 Offset;
        public Vector4 Border;
        public float PixelsToUnits;
        public Vector2 Pivot = new(0.5f, 0.5f);
        public uint Extrude;
        public bool IsPolygon;
        public KeyValuePair<Guid, long> RenderDataKey;
        public string[] AtlasTags;
        public PPtr<SpriteAtlas> SpriteAtlas;
        public SpriteRenderData RD;
        public Vector2[][] PhysicsShape;
    }
}
