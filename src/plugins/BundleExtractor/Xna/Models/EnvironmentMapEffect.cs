using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ZoDream.BundleExtractor.Xna.Models
{
    internal class EnvironmentMapEffect
    {
        public string Texture { get; internal set; }
        public string EnvironmentMap { get; internal set; }
        public float EnvironmentMapAmount { get; internal set; }
        public Vector3 EnvironmentMapSpecular { get; internal set; }
        public float FresnelFactor { get; internal set; }
        public Vector3 DiffuseColor { get; internal set; }
        public Vector3 EmissiveColor { get; internal set; }
        public float Alpha { get; internal set; }
    }
}
