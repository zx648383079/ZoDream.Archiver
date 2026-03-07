using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ZoDream.BundleExtractor.Xna.Models
{
    internal class DualTextureEffect
    {
        public string Texture { get; internal set; }
        public string Texture2 { get; internal set; }
        public Vector3 DiffuseColor { get; internal set; }
        public float Alpha { get; internal set; }
        public bool VertexColorEnabled { get; internal set; }
    }
}
