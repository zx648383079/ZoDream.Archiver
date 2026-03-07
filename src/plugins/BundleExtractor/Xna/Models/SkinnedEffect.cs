using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ZoDream.BundleExtractor.Xna.Models
{
    internal class SkinnedEffect
    {
        public string Texture { get; internal set; }
        public int WeightsPerVertex { get; internal set; }
        public Vector3 DiffuseColor { get; internal set; }
        public Vector3 EmissiveColor { get; internal set; }
        public Vector3 SpecularColor { get; internal set; }
        public float SpecularPower { get; internal set; }
        public float Alpha { get; internal set; }
    }
}
