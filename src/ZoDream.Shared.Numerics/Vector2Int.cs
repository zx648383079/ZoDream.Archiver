using System;

namespace ZoDream.Shared.Numerics
{
    public struct Vector2Int : IEquatable<Vector2Int>
    {
        public int X; 
        public int Y;

        public Vector2Int()
        {
            
        }

        public Vector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override readonly string ToString()
        {
            return $"{{{X},{Y}}}";
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode();
        }

        public override readonly bool Equals(object? obj)
        {
            if (obj is Vector2Int o)
            {
                return Equals(o);
            }
            return base.Equals(obj);
        }

        public readonly bool Equals(Vector2Int other)
        {
            return other.X == X && other.Y == Y;
        }

        public static Vector2Int operator +(Vector2Int v1, Vector2Int v2)
        {
            return new(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vector2Int operator -(Vector2Int v1, Vector2Int v2)
        {
            return new(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector2Int operator *(Vector2Int v1, Vector2Int v2)
        {
            return new(v1.X * v2.X, v1.Y * v2.Y);
        }

        public static Vector2Int operator /(Vector2Int v1, Vector2Int v2)
        {
            return new(v1.X / v2.X, v1.Y / v2.Y);
        }

        public static Vector2Int operator +(Vector2Int v1, int v2)
        {
            return new(v1.X + v2, v1.Y + v2);
        }

        public static Vector2Int operator -(Vector2Int v1, int v2)
        {
            return new(v1.X - v2, v1.Y - v2);
        }

        public static Vector2Int operator *(Vector2Int v1, int v2)
        {
            return new(v1.X * v2, v1.Y * v2);
        }

        public static Vector2Int operator /(Vector2Int v1, int v2)
        {
            return new(v1.X / v2, v1.Y / v2);
        }

        public static bool operator ==(Vector2Int v1, Vector2Int v2)
        {
            return v1.Equals(v2);
        }

        public static bool operator !=(Vector2Int v1, Vector2Int v2)
        {
            return !v1.Equals(v2);
        }
    }
}
