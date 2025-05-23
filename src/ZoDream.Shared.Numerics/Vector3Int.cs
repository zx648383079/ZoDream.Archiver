﻿using System;

namespace ZoDream.Shared.Numerics
{
    public struct Vector3Int : IEquatable<Vector3Int>
    {
        public int X;
        public int Y;
        public int Z;

        public Vector3Int()
        {
            
        }

        public Vector3Int(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3Int(Vector2Int vec, int z = 1)
            : this (vec.X, vec.Y, z)
        {
            
        }

        public override readonly string ToString()
        {
            return $"{{{X},{Y},{Z}}}";
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public override readonly bool Equals(object? obj)
        {
            if (obj is Vector3Int o)
            {
                return Equals(o);
            }
            return base.Equals(obj);
        }

        public readonly bool Equals(Vector3Int other)
        {
            return other.X == X && other.Y == Y && other.Z == Z;
        }
        public static bool operator ==(Vector3Int left, Vector3Int right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3Int left, Vector3Int right)
        {
            return !(left == right);
        }
    }
}
