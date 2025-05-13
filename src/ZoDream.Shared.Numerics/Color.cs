using System;
using System.Numerics;

namespace ZoDream.Shared.Numerics
{
    public struct Color: IEquatable<Color>
    {
        public byte A;
        public byte B;
        public byte G;
        public byte R;

        public Color()
        {
            
        }

        public Color(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Color(float r, float g, float b, float a = 1)
        {
            R = (byte)(r * 255);
            G = (byte)(g * 255);
            B = (byte)(b * 255);
            A = (byte)(a * 255);
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
            if (obj is Color o)
            {
                return Equals(o);
            }
            return base.Equals(obj);
        }

        public readonly bool Equals(Color other)
        {
            return other.R == R && other.G == G && other.B == B && other.A == A;
        }

        public static explicit operator Color(Vector3 vec)
        {
            return new(vec.X, vec.Y, vec.Z);
        }

        public static explicit operator Color(Vector4 vec)
        {
            return new(vec.X, vec.Y, vec.Z, vec.W);
        }

        public static explicit operator Vector4(Color color)
        {
            return new((float)color.R / 255, (float)color.G / 255, (float)color.B / 255, (float)color.A / 255);
        }

        public static explicit operator Color(Vector3Int vec)
        {
            return new((byte)vec.X, (byte)vec.Y, (byte)vec.Z);
        }
    }
}
