using System;
using System.Drawing;
using System.Numerics;

namespace ZoDream.Shared.Numerics
{
    public struct ColorF: IEquatable<ColorF>
    {
        public float A;
        public float B;
        public float G;
        public float R;

        public ColorF()
        {
            
        }

        public ColorF(byte r, byte g, byte b, byte a = 255)
        {
            R = (float)r / 255;
            G = (float)g / 255;
            B = (float)b / 255;
            A = (float)a / 255;
        }

        public ColorF(float r, float g, float b, float a = 1)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public override readonly string ToString()
        {
            return $"[{R},{G},{B},{A}]";
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(R, G, B, A);
        }

        public override readonly bool Equals(object? obj)
        {
            if (obj is ColorF o)
            {
                return Equals(o);
            }
            return base.Equals(obj);
        }

        public readonly bool Equals(ColorF other)
        {
            return other.R == R && other.G == G && other.B == B && other.A == A;
        }

        public static explicit operator ColorF(Vector3 vec)
        {
            return new(vec.X, vec.Y, vec.Z);
        }

        public static explicit operator ColorF(Vector4 vec)
        {
            return new(vec.X, vec.Y, vec.Z, vec.W);
        }

        public static explicit operator Vector4(ColorF color)
        {
            return new(color.R, color.G, color.B, color.A);
        }

        public static explicit operator ColorF(Vector3Int vec)
        {
            return new((byte)vec.X, (byte)vec.Y, (byte)vec.Z);
        }

        public static explicit operator ColorF(Color color)
        {
            return new(color.R, color.G, color.B, color.A);
        }
        public static explicit operator Color(ColorF color)
        {
            return new(color.R, color.G, color.B, color.A);
        }
    }
}
