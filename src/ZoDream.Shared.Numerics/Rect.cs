using System;
using System.Numerics;

namespace ZoDream.Shared.Numerics
{
    public struct Rect: IEquatable<Rect>
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public Rect()
        {
            
        }

        public Rect(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public readonly bool Equals(Rect other)
        {
            return other.X == X && other.Y == Y && other.Width == Width && other.Height == Height;
        }

        public override readonly string ToString()
        {
            return $"{{{X},{Y},{Width},{Height}}}";
        }

        public static explicit operator Rect(Vector4 vec)
        {
            return new(vec.X, vec.Y, vec.Z, vec.W);
        }

        public static explicit operator Vector4(Rect rect)
        {
            return new(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static explicit operator Rect(Quaternion vec)
        {
            return new(vec.X, vec.Y, vec.Z, vec.W);
        }

        public static explicit operator Quaternion(Rect rect)
        {
            return new(rect.X, rect.Y, rect.Width, rect.Height);
        }
    }
}
