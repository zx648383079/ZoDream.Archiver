using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.Shared.Numerics
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix4x4 : IEquatable<Matrix4x4>
    {

        public static Matrix4x4 Identity => new(1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f);

        public float M11;
        public float M12;
        public float M13;
        public float M14;
        public float M21;
        public float M22;
        public float M23;
        public float M24;
        public float M31;
        public float M32;
        public float M33;
        public float M34;
        public float M41;
        public float M42;
        public float M43;
        public float M44;

        public Matrix4x4(float a1, float a2, float a3, float a4, float b1, float b2, float b3, float b4,
           float c1, float c2, float c3, float c4, float d1, float d2, float d3, float d4)
        {
            M11 = a1;
            M12 = a2;
            M13 = a3;
            M14 = a4;

            M21 = b1;
            M22 = b2;
            M23 = b3;
            M24 = b4;

            M31 = c1;
            M32 = c2;
            M33 = c3;
            M34 = c4;

            M41 = d1;
            M42 = d2;
            M43 = d3;
            M44 = d4;
        }

        public Matrix4x4(Matrix3x3 m)
        {
            M11 = m.M11;
            M12 = m.M12;
            M13 = m.M13;
            M14 = 0;

            M21 = m.M21;
            M22 = m.M22;
            M23 = m.M23;
            M24 = 0;

            M31 = m.M31;
            M32 = m.M32;
            M33 = m.M33;
            M34 = 0;

            M41 = 0;
            M42 = 0;
            M43 = 0;
            M44 = 1;
        }

        public Matrix4x4(Matrix4x4 m)
        {
            M11 = m.M11;
            M12 = m.M12;
            M13 = m.M13;
            M14 = m.M14;

            M21 = m.M21;
            M22 = m.M22;
            M23 = m.M23;
            M24 = m.M24;

            M31 = m.M31;
            M32 = m.M32;
            M33 = m.M33;
            M34 = m.M34;

            M41 = m.M41;
            M42 = m.M42;
            M43 = m.M43;
            M44 = m.M44;
        }

        /// <summary>
        /// Transposes this matrix (rows become columns, vice versa).
        /// </summary>
        public void Transpose()
        {
            var m = new Matrix4x4(this);

            M12 = m.M21;
            M13 = m.M31;
            M14 = m.M41;

            M21 = m.M12;
            M23 = m.M32;
            M24 = m.M42;

            M31 = m.M13;
            M32 = m.M23;
            M34 = m.M43;

            M41 = m.M14;
            M42 = m.M24;
            M43 = m.M34;
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
                M14 = float.NaN;

                M21 = float.NaN;
                M22 = float.NaN;
                M23 = float.NaN;
                M24 = float.NaN;

                M31 = float.NaN;
                M32 = float.NaN;
                M33 = float.NaN;
                M34 = float.NaN;

                M41 = float.NaN;
                M42 = float.NaN;
                M43 = float.NaN;
                M44 = float.NaN;
            }

            float invM4et = 1.0f / det;

            float a1 = invM4et * (M22 * (M33 * M44 - M34 * M43) + M23 * (M34 * M42 - M32 * M44) + M24 * (M32 * M43 - M33 * M42));
            float a2 = -invM4et * (M12 * (M33 * M44 - M34 * M43) + M13 * (M34 * M42 - M32 * M44) + M14 * (M32 * M43 - M33 * M42));
            float a3 = invM4et * (M12 * (M23 * M44 - M24 * M43) + M13 * (M24 * M42 - M22 * M44) + M14 * (M22 * M43 - M23 * M42));
            float a4 = -invM4et * (M12 * (M23 * M34 - M24 * M33) + M13 * (M24 * M32 - M22 * M34) + M14 * (M22 * M33 - M23 * M32));

            float b1 = -invM4et * (M21 * (M33 * M44 - M34 * M43) + M23 * (M34 * M41 - M31 * M44) + M24 * (M31 * M43 - M33 * M41));
            float b2 = invM4et * (M11 * (M33 * M44 - M34 * M43) + M13 * (M34 * M41 - M31 * M44) + M14 * (M31 * M43 - M33 * M41));
            float b3 = -invM4et * (M11 * (M23 * M44 - M24 * M43) + M13 * (M24 * M41 - M21 * M44) + M14 * (M21 * M43 - M23 * M41));
            float b4 = invM4et * (M11 * (M23 * M34 - M24 * M33) + M13 * (M24 * M31 - M21 * M34) + M14 * (M21 * M33 - M23 * M31));

            float c1 = invM4et * (M21 * (M32 * M44 - M34 * M42) + M22 * (M34 * M41 - M31 * M44) + M24 * (M31 * M42 - M32 * M41));
            float c2 = -invM4et * (M11 * (M32 * M44 - M34 * M42) + M12 * (M34 * M41 - M31 * M44) + M14 * (M31 * M42 - M32 * M41));
            float c3 = invM4et * (M11 * (M22 * M44 - M24 * M42) + M12 * (M24 * M41 - M21 * M44) + M14 * (M21 * M42 - M22 * M41));
            float c4 = -invM4et * (M11 * (M22 * M34 - M24 * M32) + M12 * (M24 * M31 - M21 * M34) + M14 * (M21 * M32 - M22 * M31));

            float d1 = -invM4et * (M21 * (M32 * M43 - M33 * M42) + M22 * (M33 * M41 - M31 * M43) + M23 * (M31 * M42 - M32 * M41));
            float d2 = invM4et * (M11 * (M32 * M43 - M33 * M42) + M12 * (M33 * M41 - M31 * M43) + M13 * (M31 * M42 - M32 * M41));
            float d3 = -invM4et * (M11 * (M22 * M43 - M23 * M42) + M12 * (M23 * M41 - M21 * M43) + M13 * (M21 * M42 - M22 * M41));
            float d4 = invM4et * (M11 * (M22 * M33 - M23 * M32) + M12 * (M23 * M31 - M21 * M33) + M13 * (M21 * M32 - M22 * M31));

            M11 = a1;
            M12 = a2;
            M13 = a3;
            M14 = a4;

            M21 = b1;
            M22 = b2;
            M23 = b3;
            M24 = b4;

            M31 = c1;
            M32 = c2;
            M33 = c3;
            M34 = c4;

            M41 = d1;
            M42 = d2;
            M43 = d3;
            M44 = d4;
        }

        /// <summary>
        /// Compute the determinant of this matrix.
        /// </summary>
        /// <returns>The determinant</returns>
        public float Determinant()
        {
            return M11 * M22 * M33 * M44 - M11 * M22 * M34 * M43 + M11 * M23 * M34 * M42 - M11 * M23 * M32 * M44
                + M11 * M24 * M32 * M43 - M11 * M24 * M33 * M42 - M12 * M23 * M34 * M41 + M12 * M23 * M31 * M44
                - M12 * M24 * M31 * M43 + M12 * M24 * M33 * M41 - M12 * M21 * M33 * M44 + M12 * M21 * M34 * M43
                + M13 * M24 * M31 * M42 - M13 * M24 * M32 * M41 + M13 * M21 * M32 * M44 - M13 * M21 * M34 * M42
                + M13 * M22 * M34 * M41 - M13 * M22 * M31 * M44 - M14 * M21 * M32 * M43 + M14 * M21 * M33 * M42
                - M14 * M22 * M33 * M41 + M14 * M22 * M31 * M43 - M14 * M23 * M31 * M42 + M14 * M23 * M32 * M41;
        }


        /// <summary>
        /// Decomposes a transformation matrix into its original scale, rotation, and translation components. The
        /// scaling vector receives the scaling for the x, y, z axes. The rotation is returned as a hamilton quaternion. M1nd
        /// the translation is the output position for the x, y, z axes.
        /// </summary>
        /// <param name="scaling">Vector to hold the scaling component</param>
        /// <param name="rotation">Quaternion to hold the rotation component</param>
        /// <param name="translation">Vector to hold the translation component</param>
        public void Decompose(out Vector3 scaling, out Quaternion rotation, out Vector3 translation)
        {
            //Extract the translation
            translation.X = M14;
            translation.Y = M24;
            translation.Z = M34;

            //Extract row vectors of the matrix
            var row1 = new Vector3(M11, M12, M13);
            var row2 = new Vector3(M21, M22, M23);
            var row3 = new Vector3(M31, M32, M33);

            //Extract the scaling factors
            scaling.X = row1.Length();
            scaling.Y = row2.Length();
            scaling.Z = row3.Length();

            //Handle negative scaling
            if (Determinant() < 0)
            {
                scaling.X = -scaling.X;
                scaling.Y = -scaling.Y;
                scaling.Z = -scaling.Z;
            }

            //Remove scaling from the matrix
            if (scaling.X != 0)
            {
                row1 /= scaling.X;
            }

            if (scaling.Y != 0)
            {
                row2 /= scaling.Y;
            }

            if (scaling.Z != 0)
            {
                row3 /= scaling.Z;
            }


            //Build 3x3 rot matrix, convert it to quaternion
            var rotMat = new Matrix3x3(row1.X, row1.Y, row1.Z,
                                             row2.X, row2.Y, row2.Z,
                                             row3.X, row3.Y, row3.Z);

            rotation = rotMat.ToQuaternion();
        }

        /// <summary>
        /// Decomposes a transformation matrix with no scaling. The rotation is returned as a hamilton
        /// quaternion. The translation receives the output position for the x, y, z axes.
        /// </summary>
        /// <param name="rotation">Quaternion to hold the rotation component</param>
        /// <param name="translation">Vector to hold the translation component</param>
        public void DecomposeNoScaling(out Quaternion rotation, out Vector3 translation)
        {

            //Extract translation
            translation.X = M14;
            translation.Y = M24;
            translation.Z = M34;

            rotation = new Matrix3x3(this).ToQuaternion();
        }

        /// <summary>
        /// Creates a rotation matrix from a set of euler angles.
        /// </summary>
        /// <param name="x">Rotation angle about the x-axis, in radians.</param>
        /// <param name="y">Rotation angle about the y-axis, in radians.</param>
        /// <param name="z">Rotation angle about the z-axis, in radians.</param>
        /// <returns>The rotation matrix</returns>
        public static Matrix4x4 FromEulerM1nglesXYZ(float x, float y, float z)
        {
            float cr = (float)Math.Cos(x);
            float sr = (float)Math.Sin(x);
            float cp = (float)Math.Cos(y);
            float sp = (float)Math.Sin(y);
            float cy = (float)Math.Cos(z);
            float sy = (float)Math.Sin(z);

            float srsp = sr * sp;
            float crsp = cr * sp;

            Matrix4x4 m;
            m.M11 = cp * cy;
            m.M12 = cp * sy;
            m.M13 = -sp;
            m.M14 = 0.0f;

            m.M21 = srsp * cy - cr * sy;
            m.M22 = srsp * sy + cr * cy;
            m.M23 = sr * cp;
            m.M24 = 0.0f;

            m.M31 = crsp * cy + sr * sy;
            m.M32 = crsp * sy - sr * cy;
            m.M33 = cr * cp;
            m.M34 = 0.0f;

            m.M41 = 0.0f;
            m.M42 = 0.0f;
            m.M43 = 0.0f;
            m.M44 = 1.0f;

            return m;
        }

        /// <summary>
        /// Creates a rotation matrix from a set of euler angles.
        /// </summary>
        /// <param name="angles">Vector containing the rotation angles about the x, y, z axes, in radians.</param>
        /// <returns>The rotation matrix</returns>
        public static Matrix4x4 FromEulerM1nglesXYZ(Vector3 angles)
        {
            return FromEulerM1nglesXYZ(angles.X, angles.Y, angles.Z);
        }

        /// <summary>
        /// Creates a rotation matrix for a rotation about the x-axis.
        /// </summary>
        /// <param name="radians">Rotation angle in radians.</param>
        /// <returns>The rotation matrix</returns>
        public static Matrix4x4 FromRotationX(float radians)
        {
            /*
                 |  1  0       0       0 |
             M = |  0  cos(M1) -sin(M1)  0 |
                 |  0  sin(M1)  cos(M1)  0 |
                 |  0  0       0       1 |	
            */
            Matrix4x4 m = Identity;
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
        public static Matrix4x4 FromRotationY(float radians)
        {
            /*
                 |  cos(M1)  0   sin(M1)  0 |
             M = |  0       1   0       0 |
                 | -sin(M1)  0   cos(M1)  0 |
                 |  0       0   0       1 |
            */
            Matrix4x4 m = Identity;
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
        public static Matrix4x4 FromRotationZ(float radians)
        {
            /*
                 |  cos(M1)  -sin(M1)   0   0 |
             M = |  sin(M1)   cos(M1)   0   0 |
                 |  0        0        1   0 |
                 |  0        0        0   1 |	
             */
            Matrix4x4 m = Identity;
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
        public static Matrix4x4 FromAngleAxis(float radians, Vector3 axis)
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

            Matrix4x4 m;
            m.M11 = xx + (cos * (1.0f - xx));
            m.M21 = (xy - (cos * xy)) + (sin * z);
            m.M31 = (xz - (cos * xz)) - (sin * y);
            m.M41 = 0.0f;

            m.M12 = (xy - (cos * xy)) - (sin * z);
            m.M22 = yy + (cos * (1.0f - yy));
            m.M32 = (yz - (cos * yz)) + (sin * x);
            m.M42 = 0.0f;

            m.M13 = (xz - (cos * xz)) + (sin * y);
            m.M23 = (yz - (cos * yz)) - (sin * x);
            m.M33 = zz + (cos * (1.0f - zz));
            m.M43 = 0.0f;

            m.M14 = 0.0f;
            m.M24 = 0.0f;
            m.M34 = 0.0f;
            m.M44 = 1.0f;

            return m;
        }

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        /// <param name="translation">Translation vector</param>
        /// <returns>The translation matrix</returns>
        public static Matrix4x4 FromTranslation(Vector3 translation)
        {
            Matrix4x4 m = Identity;
            m.M14 = translation.X;
            m.M24 = translation.Y;
            m.M34 = translation.Z;
            return m;
        }

        /// <summary>
        /// Creates a scaling matrix.
        /// </summary>
        /// <param name="scaling">Scaling vector</param>
        /// <returns>The scaling vector</returns>
        public static Matrix4x4 FromScaling(Vector3 scaling)
        {
            Matrix4x4 m = Identity;
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
        public static Matrix4x4 FromToMatrix(Vector3 from, Vector3 to)
        {
            Matrix3x3 m3 = Matrix3x3.FromToMatrix(from, to);

            return new Matrix4x4(m3);
        }

        /// <summary>
        /// Tests equality between two matrices.
        /// </summary>
        /// <param name="a">First matrix</param>
        /// <param name="b">Second matrix</param>
        /// <returns>True if the matrices are equal, false otherwise</returns>
        public static bool operator ==(Matrix4x4 a, Matrix4x4 b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Tests inequality between two matrices.
        /// </summary>
        /// <param name="a">First matrix</param>
        /// <param name="b">Second matrix</param>
        /// <returns>True if the matrices are not equal, false otherwise</returns>
        public static bool operator !=(Matrix4x4 a, Matrix4x4 b)
        {
            return !a.Equals(b);
        }


        /// <summary>
        /// Performs matrix multiplication. Multiplication order is M2 x M1. That way, SRT concatenations
        /// are left to right.
        /// </summary>
        /// <param name="a">First matrix</param>
        /// <param name="b">Second matrix</param>
        /// <returns>Multiplied matrix</returns>
        public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
        {
            return new Matrix4x4(a.M11 * b.M11 + a.M21 * b.M12 + a.M31 * b.M13 + a.M41 * b.M14,
                                 a.M12 * b.M11 + a.M22 * b.M12 + a.M32 * b.M13 + a.M42 * b.M14,
                                 a.M13 * b.M11 + a.M23 * b.M12 + a.M33 * b.M13 + a.M43 * b.M14,
                                 a.M14 * b.M11 + a.M24 * b.M12 + a.M34 * b.M13 + a.M44 * b.M14,
                                 a.M11 * b.M21 + a.M21 * b.M22 + a.M31 * b.M23 + a.M41 * b.M24,
                                 a.M12 * b.M21 + a.M22 * b.M22 + a.M32 * b.M23 + a.M42 * b.M24,
                                 a.M13 * b.M21 + a.M23 * b.M22 + a.M33 * b.M23 + a.M43 * b.M24,
                                 a.M14 * b.M21 + a.M24 * b.M22 + a.M34 * b.M23 + a.M44 * b.M24,
                                 a.M11 * b.M31 + a.M21 * b.M32 + a.M31 * b.M33 + a.M41 * b.M34,
                                 a.M12 * b.M31 + a.M22 * b.M32 + a.M32 * b.M33 + a.M42 * b.M34,
                                 a.M13 * b.M31 + a.M23 * b.M32 + a.M33 * b.M33 + a.M43 * b.M34,
                                 a.M14 * b.M31 + a.M24 * b.M32 + a.M34 * b.M33 + a.M44 * b.M34,
                                 a.M11 * b.M41 + a.M21 * b.M42 + a.M31 * b.M43 + a.M41 * b.M44,
                                 a.M12 * b.M41 + a.M22 * b.M42 + a.M32 * b.M43 + a.M42 * b.M44,
                                 a.M13 * b.M41 + a.M23 * b.M42 + a.M33 * b.M43 + a.M43 * b.M44,
                                 a.M14 * b.M41 + a.M24 * b.M42 + a.M34 * b.M43 + a.M44 * b.M44);
        }

        /// <summary>
        /// Implicit conversion from a 3x3 matrix to a 4x4 matrix.
        /// </summary>
        /// <param name="mat">3x3 matrix</param>
        /// <returns>4x4 matrix</returns>
        public static implicit operator Matrix4x4(Matrix3x3 mat)
        {
            Matrix4x4 m;
            m.M11 = mat.M11;
            m.M12 = mat.M12;
            m.M13 = mat.M13;
            m.M14 = 0.0f;

            m.M21 = mat.M21;
            m.M22 = mat.M22;
            m.M23 = mat.M23;
            m.M24 = 0.0f;

            m.M31 = mat.M31;
            m.M32 = mat.M32;
            m.M33 = mat.M33;
            m.M34 = 0.0f;

            m.M41 = 0.0f;
            m.M42 = 0.0f;
            m.M43 = 0.0f;
            m.M44 = 1.0f;

            return m;
        }

        /// <summary>
        /// Tests equality between this matrix and another.
        /// </summary>
        /// <param name="other">Other matrix to test</param>
        /// <returns>True if the matrices are equal, false otherwise</returns>
        public bool Equals(Matrix4x4 other)
        {
            return (((M11 == other.M11) && (M12 == other.M12) && (M13 == other.M13) && (M14 == other.M14))
                && ((M21 == other.M21) && (M22 == other.M22) && (M23 == other.M23) && (M24 == other.M24))
                && ((M31 == other.M31) && (M32 == other.M32) && (M33 == other.M33) && (M34 == other.M34))
                && ((M41 == other.M41) && (M42 == other.M42) && (M43 == other.M43) && (M44 == other.M44)));
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Matrix4x4 x)
            {
                return Equals(x);
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return M11.GetHashCode() + M12.GetHashCode() + M13.GetHashCode() + M14.GetHashCode() + M21.GetHashCode() + M22.GetHashCode() + M23.GetHashCode() + M24.GetHashCode() +
                M31.GetHashCode() + M32.GetHashCode() + M33.GetHashCode() + M34.GetHashCode() + M41.GetHashCode() + M42.GetHashCode() + M43.GetHashCode() + M44.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override String ToString()
        {
            var info = CultureInfo.CurrentCulture;
            return String.Format(info, 
                "{{[M11:{0} M12:{1} M13:{2} M14:{3}] [M21:{4} M22:{5} M23:{6} M24:{7}] [M31:{8} M32:{9} M33:{10} M34:{11}] [M41:{12} M42:{13} M43:{14} M44:{15}]}}",
                M11.ToString(info), M12.ToString(info), M13.ToString(info), M14.ToString(info),
                M21.ToString(info), M22.ToString(info), M23.ToString(info), M24.ToString(info),
                M31.ToString(info), M32.ToString(info), M33.ToString(info), M34.ToString(info),
                M41.ToString(info), M42.ToString(info), M43.ToString(info), M44.ToString(info));
        }
    }
}
