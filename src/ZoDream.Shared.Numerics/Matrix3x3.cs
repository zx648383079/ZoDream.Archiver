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
    public struct Matrix3x3 : IEquatable<Matrix3x3>, IEnumerable<float>
    {
        public static Matrix3x3 Identity => new(1.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f);

        public float M11;
        public float M12;
        public float M13;
        public float M21;
        public float M22;
        public float M23;
        public float M31;
        public float M32;
        public float M33;


        public Matrix3x3()
        {

        }

        public Matrix3x3(Matrix3x3 m)
        {
            M11 = m.M11;
            M12 = m.M12;
            M13 = m.M13;

            M21 = m.M21;
            M22 = m.M22;
            M23 = m.M23;

            M31 = m.M31;
            M32 = m.M32;
            M33 = m.M33;
        }

        public Matrix3x3(Matrix4x4 m)
        {
            M11 = m.M11;
            M12 = m.M12;
            M13 = m.M13;

            M21 = m.M21;
            M22 = m.M22;
            M23 = m.M23;

            M31 = m.M31;
            M32 = m.M32;
            M33 = m.M33;
        }

        public Matrix3x3(Matrix.Matrix4x4 m)
        {
            M11 = m.M11;
            M12 = m.M12;
            M13 = m.M13;

            M21 = m.M21;
            M22 = m.M22;
            M23 = m.M23;

            M31 = m.M31;
            M32 = m.M32;
            M33 = m.M33;
        }

        public Matrix3x3(Vector3 col1, Vector3 col2, Vector3 col3)
        {
            M11 = col1.X;
            M12 = col2.X;
            M13 = col3.X;
            M21 = col1.Y;
            M22 = col2.Y;
            M23 = col3.Y;
            M31 = col1.Z;
            M32 = col2.Z;
            M33 = col3.Z;
        }

        public Matrix3x3(float m11, float m12, float m13,
                         float m21, float m22, float m23,
                         float m31, float m32, float m33)
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M21 = m21;
            M22 = m22;
            M23 = m23;
            M31 = m31;
            M32 = m32;
            M33 = m33;
        }

        public readonly float this[int i, int j] {
            get => (float)GetType().GetField($"M{i + 1}{j + 1}")?.GetValue(this);
            set => GetType().GetField($"M{i + 1}{j + 1}")?.SetValue(this, value);
        }

        public readonly Vector3 Row(int i)
        {
            return i switch
            {
                0 => new Vector3(M11, M12, M13),
                1 => new Vector3(M21, M22, M23),
                _ => new Vector3(M31, M32, M33),
            };
        }

        public readonly Vector3 Col(int i)
        {
            return i switch
            {
                0 => new Vector3(M11, M21, M31),
                1 => new Vector3(M12, M22, M32),
                _ => new Vector3(M13, M23, M33),
            };
        }

        /// <summary>
        /// Transposes this matrix (rows become columns, vice versa).
        /// </summary>
        public readonly Matrix3x3 Transpose()
        {
            return new(M11, M21, M31, M12, M22, M32, M13, M23, M33);
        }

        /// <summary>
        /// Inverts the matrix. If the matrix is *not* invertible all elements are set to <see cref="float.NaN"/>.
        /// </summary>
        public void Inverse()
        {
            float det = Determinant();
            if (det == 0.0f)
            {
                // Matrix not invertible. Setting all elements to NaN is not really
                // correct in a mathematical sense but it is easy to debug for the
                // programmer.
                M11 = float.NaN;
                M12 = float.NaN;
                M13 = float.NaN;

                M21 = float.NaN;
                M22 = float.NaN;
                M23 = float.NaN;

                M31 = float.NaN;
                M32 = float.NaN;
                M33 = float.NaN;
            }

            float invDet = 1.0f / det;

            float a1 = invDet * (M22 * M33 - M23 * M32);
            float a2 = -invDet * (M12 * M33 - M13 * M32);
            float a3 = invDet * (M12 * M23 - M13 * M22);

            float b1 = -invDet * (M21 * M33 - M23 * M31);
            float b2 = invDet * (M11 * M33 - M13 * M31);
            float b3 = -invDet * (M11 * M23 - M13 * M21);

            float c1 = invDet * (M21 * M32 - M22 * M31);
            float c2 = -invDet * (M11 * M32 - M12 * M31);
            float c3 = invDet * (M11 * M22 - M12 * M21);

            M11 = a1;
            M12 = a2;
            M13 = a3;

            M21 = b1;
            M22 = b2;
            M23 = b3;

            M31 = c1;
            M32 = c2;
            M33 = c3;
        }

        /// <summary>
        /// Compute the determinant of this matrix.
        /// </summary>
        /// <returns>The determinant</returns>
        public float Determinant()
        {
            return M11 * M22 * M33 - M11 * M23 * M32 + M12 * M23 * M31 - M12 * M21 * M33 + M13 * M21 * M32 - M13 * M22 * M31;
        }

        public readonly bool Equals(Matrix3x3 other)
        {
            return (M11 == other.M11) && (M12 == other.M12) && (M13 == other.M13)
                && (M21 == other.M21) && (M22 == other.M22) && (M23 == other.M23)
                && (M31 == other.M31) && (M32 == other.M32) && (M33 == other.M33);
        }

        public override readonly bool Equals(object? obj)
        {
            if (obj is Matrix3x3 x)
            {
                return Equals(x);
            }
            return false;
        }

        public override readonly int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(M11);
            hash.Add(M12);
            hash.Add(M13);
            hash.Add(M21);
            hash.Add(M22);
            hash.Add(M23);
            hash.Add(M31);
            hash.Add(M32);
            hash.Add(M33);
            return hash.ToHashCode();
        }

        public override string ToString()
        {
            CultureInfo info = CultureInfo.CurrentCulture;
            return string.Format(info, 
                "{{[M11:{0} M12:{1} M13:{2}] [M21:{3} M22:{4} M23:{5}] [M31:{6} M32:{7} M33:{8}]}}",
                M11.ToString(info), M12.ToString(info), M13.ToString(info),
                M21.ToString(info), M22.ToString(info), M23.ToString(info),
                M31.ToString(info), M32.ToString(info), M33.ToString(info)
                );
        }

        public Quaternion ToQuaternion()
        {
            float trace = M11 + M22 + M33;
            var res = new Quaternion();
            if (trace > 0)
            {
                float s = (float)Math.Sqrt(trace + 1.0f) * 2.0f;
                res.W = .25f * s;
                res.X = (M32 - M23) / s;
                res.Y = (M13 - M31) / s;
                res.Z = (M21 - M12) / s;
            }
            else if ((M11 > M22) && (M11 > M33))
            {
                float s = (float)Math.Sqrt(1.0 + M11 - M22 - M33) * 2.0f;
                res.W = (M32 - M23) / s;
                res.X = .25f * s;
                res.Y = (M12 + M21) / s;
                res.Z = (M13 + M31) / s;
            }
            else if (M22 > M33)
            {
                float s = (float)Math.Sqrt(1.0f + M22 - M11 - M33) * 2.0f;
                res.W = (M13 - M31) / s;
                res.X = (M12 + M21) / s;
                res.Y = .25f * s;
                res.Z = (M23 + M32) / s;
            }
            else
            {
                float s = (float)Math.Sqrt(1.0f + M33 - M11 - M22) * 2.0f;
                res.W = (M21 - M12) / s;
                res.X = (M13 + M31) / s;
                res.Y = (M23 + M32) / s;
                res.Z = .25f * s;
            }

            return Quaternion.Normalize(res);
        }

        /// <summary>
        /// Creates a rotation matrix from a set of euler angles.
        /// </summary>
        /// <param name="x">Rotation angle about the x-axis, in radians.</param>
        /// <param name="y">Rotation angle about the y-axis, in radians.</param>
        /// <param name="z">Rotation angle about the z-axis, in radians.</param>
        /// <returns>The rotation matrix</returns>
        public static Matrix3x3 FromEulerAnglesXYZ(float x, float y, float z)
        {
            float cr = (float)Math.Cos(x);
            float sr = (float)Math.Sin(x);
            float cp = (float)Math.Cos(y);
            float sp = (float)Math.Sin(y);
            float cy = (float)Math.Cos(z);
            float sy = (float)Math.Sin(z);

            float srsp = sr * sp;
            float crsp = cr * sp;

            Matrix3x3 m;
            m.M11 = cp * cy;
            m.M12 = cp * sy;
            m.M13 = -sp;

            m.M21 = srsp * cy - cr * sy;
            m.M22 = srsp * sy + cr * cy;
            m.M23 = sr * cp;

            m.M31 = crsp * cy + sr * sy;
            m.M32 = crsp * sy - sr * cy;
            m.M33 = cr * cp;

            return m;
        }

        /// <summary>
        /// Creates a rotation matrix from a set of euler angles.
        /// </summary>
        /// <param name="angles">Vector containing the rotation angles about the x, y, z axes, in radians.</param>
        /// <returns>The rotation matrix</returns>
        public static Matrix3x3 FromEulerAnglesXYZ(Vector3 angles)
        {
            return FromEulerAnglesXYZ(angles.X, angles.Y, angles.Z);
        }

        /// <summary>
        /// Creates a rotation matrix for a rotation about the x-axis.
        /// </summary>
        /// <param name="radians">Rotation angle in radians.</param>
        /// <returns>The rotation matrix</returns>
        public static Matrix3x3 FromRotationX(float radians)
        {
            /*
                 |  1  0       0      |
             M = |  0  cos(M1) -sin(M1) |
                 |  0  sin(M1)  cos(M1) |	
            */
            Matrix3x3 m = Identity;
            m.M22 = m.M33 = (float)Math.Cos(radians);
            m.M32 = (float)Math.Sin(radians);
            m.M23 = -m.M32;
            return m;
        }

        /// <summary>
        /// Creates a rotation matrix for a rotation about the y-axis.
        /// </summary>
        /// <param name="radians">Rotation angle in radians.</param>
        /// <returns>The rotation matrix</returns>
        public static Matrix3x3 FromRotationY(float radians)
        {
            /*
                 |  cos(M1)  0   sin(M1) |
             M = |  0       1   0      |
                 | -sin(M1)  0   cos(M1) |
            */
            Matrix3x3 m = Identity;
            m.M11 = m.M33 = (float)Math.Cos(radians);
            m.M13 = (float)Math.Sin(radians);
            m.M31 = -m.M13;
            return m;
        }

        /// <summary>
        /// Creates a rotation matrix for a rotation about the z-axis.
        /// </summary>
        /// <param name="radians">Rotation angle in radians.</param>
        /// <returns>The rotation matrix</returns>
        public static Matrix3x3 FromRotationZ(float radians)
        {
            /*
                 |  cos(M1)  -sin(M1)   0 |
             M = |  sin(M1)   cos(M1)   0 |
                 |  0        0        1 |
             */
            Matrix3x3 m = Identity;
            m.M11 = m.M22 = (float)Math.Cos(radians);
            m.M21 = (float)Math.Sin(radians);
            m.M12 = -m.M21;
            return m;
        }

        /// <summary>
        /// Creates a rotation matrix for a rotation about an arbitrary axis.
        /// </summary>
        /// <param name="radians">Rotation angle, in radians</param>
        /// <param name="axis">Rotation axis, which should be a normalized vector.</param>
        /// <returns>The rotation matrix</returns>
        public static Matrix3x3 FromAngleAxis(float radians, Vector3 axis)
        {
            float x = axis.X;
            float y = axis.Y;
            float z = axis.Z;

            float sin = (float)Math.Sin((double)radians);
            float cos = (float)Math.Cos((double)radians);

            float xx = x * x;
            float yy = y * y;
            float zz = z * z;
            float xy = x * y;
            float xz = x * z;
            float yz = y * z;

            Matrix3x3 m;
            m.M11 = xx + (cos * (1.0f - xx));
            m.M21 = xy - (cos * xy) + (sin * z);
            m.M31 = xz - (cos * xz) - (sin * y);

            m.M12 = xy - (cos * xy) - (sin * z);
            m.M22 = yy + (cos * (1.0f - yy));
            m.M32 = yz - (cos * yz) + (sin * x);

            m.M13 = xz - (cos * xz) + (sin * y);
            m.M23 = yz - (cos * yz) - (sin * x);
            m.M33 = zz + (cos * (1.0f - zz));

            return m;
        }

        /// <summary>
        /// Creates a scaling matrix.
        /// </summary>
        /// <param name="scaling">Scaling vector</param>
        /// <returns>The scaling vector</returns>
        public static Matrix3x3 FromScaling(Vector3 scaling)
        {
            Matrix3x3 m = Identity;
            m.M11 = scaling.X;
            m.M22 = scaling.Y;
            m.M33 = scaling.Z;
            return m;
        }

        /// <summary>
        /// Creates a rotation matrix that rotates a vector called "from" into another
        /// vector called "to". Based on an algorithm by Tomas Moller and John Hudges:
        /// <para>
        /// "Efficiently Building a Matrix to Rotate One Vector to Another"         
        /// Journal of Graphics Tools, 4(4):1-4, 1999
        /// </para>
        /// </summary>
        /// <param name="from">Starting vector</param>
        /// <param name="to">Ending vector</param>
        /// <returns>Rotation matrix to rotate from the start to end.</returns>
        public static Matrix3x3 FromToMatrix(Vector3 from, Vector3 to)
        {
            float e = Vector3.Dot(from, to);
            float f = (e < 0) ? -e : e;

            Matrix3x3 m = Identity;

            //"from" and "to" vectors almost parallel
            if (f > 1.0f - 0.00001f)
            {
                Vector3 u, v; //Temp variables
                Vector3 x; //Vector almost orthogonal to "from"

                x.X = (from.X > 0.0f) ? from.X : -from.X;
                x.Y = (from.Y > 0.0f) ? from.Y : -from.Y;
                x.Z = (from.Z > 0.0f) ? from.Z : -from.Z;

                if (x.X < x.Y)
                {
                    if (x.X < x.Z)
                    {
                        x.X = 1.0f;
                        x.Y = 0.0f;
                        x.Z = 0.0f;
                    }
                    else
                    {
                        x.X = 0.0f;
                        x.Y = 0.0f;
                        x.Z = 1.0f;
                    }
                }
                else
                {
                    if (x.Y < x.Z)
                    {
                        x.X = 0.0f;
                        x.Y = 1.0f;
                        x.Z = 0.0f;
                    }
                    else
                    {
                        x.X = 0.0f;
                        x.Y = 0.0f;
                        x.Z = 1.0f;
                    }
                }

                u.X = x.X - from.X;
                u.Y = x.Y - from.Y;
                u.Z = x.Z - from.Z;

                v.X = x.X - to.X;
                v.Y = x.Y - to.Y;
                v.Z = x.Z - to.Z;

                float c1 = 2.0f / Vector3.Dot(u, u);
                float c2 = 2.0f / Vector3.Dot(v, v);
                float c3 = c1 * c2 * Vector3.Dot(u, v);

                for (int i = 1; i < 4; i++)
                {
                    for (int j = 1; j < 4; j++)
                    {
                        //This is somewhat unreadable, but the indices for u, v vectors are "zero-based" while
                        //matrix indices are "one-based" always subtract by one to index those
                        m[i, j] = -c1 * u.ValueOf(i - 1) * u.ValueOf(j - 1) - c2 * v.ValueOf(i - 1) * v.ValueOf(j - 1) + c3 * v.ValueOf(i - 1) * u.ValueOf(j - 1);
                    }
                    m[i, i] += 1.0f;
                }

            }
            else
            {
                //Most common case, unless "from" = "to" or "from" =- "to"
                Vector3 v = Vector3.Cross(from, to);

                //Hand optimized version (9 mults less) by Gottfried M3hen
                float h = 1.0f / (1.0f + e);
                float hvx = h * v.X;
                float hvz = h * v.Z;
                float hvxy = hvx * v.Y;
                float hvxz = hvx * v.Z;
                float hvyz = hvz * v.Y;

                m.M11 = e + hvx * v.X;
                m.M12 = hvxy - v.Z;
                m.M13 = hvxz + v.Y;

                m.M21 = hvxy + v.Z;
                m.M22 = e + h * v.Y * v.Y;
                m.M23 = hvyz - v.X;

                m.M31 = hvxz - v.Y;
                m.M32 = hvyz + v.X;
                m.M33 = e + hvz * v.Z;
            }

            return m;
        }

        public readonly IEnumerator<float> GetEnumerator()
        {
            yield return M11;
            yield return M12;
            yield return M13;
            yield return M21;
            yield return M22;
            yield return M23;
            yield return M31;
            yield return M32;
            yield return M33;
        }

        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static Matrix3x3 operator *(Matrix3x3 m1, Matrix3x3 m2)
        {
            return new Matrix3x3
            {
                M11 = m1.M11 * m2.M11 + m1.M12 * m2.M21 + m1.M13 * m2.M31,
                M12 = m1.M11 * m2.M12 + m1.M12 * m2.M22 + m1.M13 * m2.M32,
                M13 = m1.M11 * m2.M13 + m1.M12 * m2.M23 + m1.M13 * m2.M33,
                M21 = m1.M21 * m2.M11 + m1.M22 * m2.M21 + m1.M23 * m2.M31,
                M22 = m1.M21 * m2.M12 + m1.M22 * m2.M22 + m1.M23 * m2.M32,
                M23 = m1.M21 * m2.M13 + m1.M22 * m2.M23 + m1.M23 * m2.M33,
                M31 = m1.M31 * m2.M11 + m1.M32 * m2.M21 + m1.M33 * m2.M31,
                M32 = m1.M31 * m2.M12 + m1.M32 * m2.M22 + m1.M33 * m2.M32,
                M33 = m1.M31 * m2.M13 + m1.M32 * m2.M23 + m1.M33 * m2.M33
            };
        }

        //public static Vector3 operator *(Matrix3x3 m, Vector3 v)
        //{
        //    return new Vector3(m.M11 * v.X + m.M12 * v.Y + m.M13 * v.Z,
        //        m.M21 * v.X + m.M22 * v.Y + m.M23 * v.Z,
        //        m.M31 * v.X + m.M32 * v.Y + m.M33 * v.Z);
        //}

        public static Matrix3x3 operator *(Matrix3x3 m, Vector3 v)
        {
            return new Matrix3x3(
                m.M11 * v.X,
                m.M12 * v.X, 
                m.M13 * v.X,
                m.M21 * v.Y,
                m.M22 * v.Y,
                m.M23 * v.Y,
                m.M31 * v.Z,
                m.M32 * v.Z,
                m.M33 * v.Z);
        }

        public static Matrix3x3 operator *(Matrix3x3 m, float v)
        {
            return new Matrix3x3(
                m.M11 * v, m.M11 * v, m.M13 * v,
                m.M21 * v, m.M22 * v, m.M23 * v,
                m.M31 * v, m.M32 * v, m.M33 * v
            );
        }

        public static bool operator ==(Matrix3x3 m1, Matrix3x3 m2)
        {
            return m1.Equals(m2);
        }

        public static bool operator !=(Matrix3x3 m1, Matrix3x3 m2)
        {
            return !m1.Equals(m2);
        }
    }
}
