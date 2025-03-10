using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.Shared.Numerics
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix2x2 : IEquatable<Matrix3x3>, IEnumerable<float>
    {
        public static Matrix2x2 Identity => new();

        public float M11;
        public float M12;
        public float M21;
        public float M22;

        public Matrix2x2()
        {
            
        }

        public Matrix2x2(Vector2 col1, Vector2 col2)
        {
            M11 = col1.X;
            M12 = col2.X;
            M21 = col1.Y;
            M22 = col2.Y;
        }

        public Matrix2x2(float m11, float m12,
                         float m21, float m22)
        {
            M11 = m11;
            M12 = m12;
            M21 = m21;
            M22 = m22;
        }

        public bool Equals(Matrix3x3 other)
        {
            return (M11 == other.M11) && (M12 == other.M12)
                && (M21 == other.M21) && (M22 == other.M22);
        }

        public override bool Equals(object obj)
        {
            if (obj is Matrix3x3 x)
            {
                return Equals(x);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return M11.GetHashCode() + M12.GetHashCode() + M21.GetHashCode() + M22.GetHashCode();
        }

        public override string ToString()
        {
            CultureInfo info = CultureInfo.CurrentCulture;
            return string.Format(info,
                "{{[M11:{0} M12:{1}] [M21:{2} M22:{3}]}}",
                M11.ToString(info), M12.ToString(info),
                M21.ToString(info), M22.ToString(info)
                );
        }

        public readonly IEnumerator<float> GetEnumerator()
        {
            yield return M11;
            yield return M12;
            yield return M21;
            yield return M22;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
