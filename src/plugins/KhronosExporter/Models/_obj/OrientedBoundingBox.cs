using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ZoDream.Shared.Numerics;

namespace ZoDream.KhronosExporter.Models
{
    internal class OrientedBoundingBox
    {
        public Matrix3x3 HalfAxis { get; set; }

        public Vector3 Center { get; set; }


        public static OrientedBoundingBox FromPoints(IList<Vector3> positions)
        {
            var result = new OrientedBoundingBox();

            var length = positions.Count;

            var meanPoint = positions[0];
            for (var i = 1; i < length; i++)
            {
                meanPoint += positions[i];
            }
            var invLength = 1.0f / length;

            meanPoint = meanPoint * invLength;

            var exx = 0.0f;
            var exy = 0.0f;
            var exz = 0.0f;
            var eyy = 0.0f;
            var eyz = 0.0f;
            var ezz = 0.0f;

            for (var i = 0; i < length; i++)
            {
                var p = positions[i] - meanPoint;
                exx += p.X * p.X;
                exy += p.X * p.Y;
                exz += p.X * p.Z;
                eyy += p.Y * p.Y;
                eyz += p.Y * p.Z;
                ezz += p.Z * p.Z;
            }

            exx *= invLength;
            exy *= invLength;
            exz *= invLength;
            eyy *= invLength;
            eyz *= invLength;
            ezz *= invLength;

            var covarianceMatrix = new Matrix3x3(exx, exy, exz, exy, eyy, eyz, exz, eyz, ezz);

            var (diagMatrix, unitaryMatrix) = ComputeEigenDecomposition(covarianceMatrix);
            var rotation = new Matrix3x3(unitaryMatrix);

            var v1 = rotation.Col(0);
            var v2 = rotation.Col(1);
            var v3 = rotation.Col(2);

            var u1 = float.MinValue; //-Number.MAX_VALUE;
            var u2 = float.MinValue; //-Number.MAX_VALUE;
            var u3 = float.MinValue; //-Number.MAX_VALUE;
            var l1 = float.MaxValue; //Number.MAX_VALUE;
            var l2 = float.MaxValue; //Number.MAX_VALUE;
            var l3 = float.MaxValue; //Number.MAX_VALUE;

            for (var i = 0; i < length; i++)
            {
                var p = positions[i];
                u1 = new[] { Vector3.Dot(v1, p), u1 }.Max();
                u2 = new[] { Vector3.Dot(v2, p), u2 }.Max();
                u3 = new[] { Vector3.Dot(v3, p), u3 }.Max();

                l1 = new[] { Vector3.Dot(v1, p), l1 }.Min();
                l2 = new[] { Vector3.Dot(v2, p), l2 }.Min();
                l3 = new[] { Vector3.Dot(v3, p), l3 }.Min();
            }

            v1 = v1 * 0.5f * (l1 + u1);
            v2 = v2 * 0.5f * (l2 + u2);
            v3 = v3 * 0.5f * (l3 + u3);

            var center = v1 + v2;
            center = center + v3;

            var scale = new Vector3(u1 - l1, u2 - l2, u3 - l3);
            scale *= 0.5f;

            rotation *= scale;

            return new OrientedBoundingBox
            {
                Center = center,
                HalfAxis = rotation
            };
        }

        private static float ComputeFrobeniusNorm(Matrix3x3 matrix)
        {
            return (float)Math.Sqrt(matrix.Select(c => c * c).Sum());
        }

        private static float OffDiagonalFrobeniusNorm(Matrix3x3 matrix)
        {
            var a = matrix.M11;
            var b = matrix.M22;
            var c = matrix.M33;
            return (float)Math.Sqrt(a * a + b * b + c * c);
        }

        private static Matrix3x3 ShurDecomposition(Matrix3x3 matrix)
        {
            int[] rowVal = [1, 0, 0];
            int[] colVal = [2, 2, 1];
            var tolerance = 1e-15;
            var maxDiagonal = 0.0;
            var rotAxis = 1;

            // find pivot (rotAxis) based on max diagonal of matrix
            for (var i = 0; i < 3; ++i)
            {
                var temp = Math.Abs(matrix[rowVal[i], colVal[i]]);
                if (temp > maxDiagonal)
                {
                    rotAxis = i;
                    maxDiagonal = temp;
                }
            }
            var c = 1.0f;
            var s = 0.0f;

            var p = rowVal[rotAxis];
            var q = colVal[rotAxis];

            if (Math.Abs(matrix[p, q]) > tolerance)
            {
                var qq = matrix[q, q];
                var pp = matrix[p, p];
                var qp = matrix[p, q];

                var tau = (qq - pp) / (2.0f * qp);
                float t;
                if (tau < 0)
                {
                    t = -1.0f / (-tau + (float)Math.Sqrt(1.0 + tau * tau));
                }
                else
                {
                    t = 1.0f / (tau + (float)Math.Sqrt(1.0 + tau * tau));
                }

                c = 1.0f / (float)Math.Sqrt(1.0f + t * t);
                s = t * c;
            }

            var result = Matrix3x3.Identity;
            result[p, p] = result[q, q] = c;
            result[p, q] = s;
            result[q, p] = -s;

            return result;
        }

        private static (Matrix3x3, Matrix3x3) ComputeEigenDecomposition(Matrix3x3 matrix)
        {
            var tolerance = 1e-20;
            var maxSweeps = 10;

            var count = 0;
            var sweep = 0;

            var epsilon = tolerance * ComputeFrobeniusNorm(matrix);

            var diagMatrix = new Matrix3x3(matrix);
            var unitaryMatrix = Matrix3x3.Identity;

            while (sweep < maxSweeps && OffDiagonalFrobeniusNorm(diagMatrix) > epsilon)
            {
                var jMatrix = ShurDecomposition(diagMatrix);
                var jMatrixTranspose = jMatrix.Transpose();
                diagMatrix *= jMatrix;
                diagMatrix = jMatrixTranspose * diagMatrix;
                unitaryMatrix *= jMatrix;

                if (++count > 2)
                {
                    ++sweep;
                    count = 0;
                }
            }

            return (diagMatrix, unitaryMatrix);
        }
    }
}
