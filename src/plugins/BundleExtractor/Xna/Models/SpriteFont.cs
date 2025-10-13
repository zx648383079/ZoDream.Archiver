using System.Numerics;
using UnityEngine;
using ZoDream.Shared.Numerics;

namespace ZoDream.BundleExtractor.Xna.Models
{
    internal class SpriteFont
    {
        public Texture2D? Texture { get; internal set; }
        public Vector4Int[] Glyphs { get; internal set; }
        public Vector4Int[] Cropping { get; internal set; }
        public char[] CharacterMap { get; internal set; }
        public int VerticalLineSpacing { get; internal set; }
        public float HorizontalSpacing { get; internal set; }
        public Vector3[] Kerning { get; internal set; }
        public object DefaultCharacter { get; internal set; }
    }
}
