using System;
using System.Numerics;

namespace ZoDream.Shared.Numerics
{
    public static class NumericsExtension
    {

        /// <summary>
        /// 根据索引获取值
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static float ValueOf(this Vector3 data, int index)
        {
            return index switch
            {
                0 => data.X,
                1 => data.Y,
                2 => data.Z,
                _ => 0
            };
        }

        public static float[] AsArray(this Vector2 data)
        {
            return [data.X, data.Y];
        }
        public static float[] AsArray(this Vector3 data)
        {
            return [data.X, data.Y, data.Z];
        }

        public static float[] AsArray(this Vector4 data)
        {
            return [data.X, data.Y, data.Z, data.W];
        }

        public static float[] AsArray(this Quaternion data)
        {
            return [data.X, data.Y, data.Z, data.W];
        }

        public static float Denormalize(float h)
        {
            return h < 0f ? h + 360f : h;
        }

        public static Vector3 Denormalize(this Vector3 v)
        {
            return new Vector3(Denormalize(v.X), Denormalize(v.Y), Denormalize(v.Z));
        }

        public static float ToRadians(float val)
        {
            return (float)(Math.PI / 180) * val;
        }

        public static float ToDegrees(float val)
        {
            return (float)(val * (180 / Math.PI));
        }


        public static Vector4 AsVector4(this Vector3 vec, float w = 0)
        {
            return new(vec.X, vec.Y, vec.Z, w);
        }

        /// <summary>
        ///  四元数转欧拉角
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static Vector3 ToEuler(this Quaternion q)
        {
            double sqw = q.W * q.W;
            double sqx = q.X * q.X;
            double sqy = q.Y * q.Y;
            double sqz = q.Z * q.Z;

            return new Vector3(
                Denormalize(ToDegrees(
                    (float)Math.Asin(2f * (q.X * q.Z - q.W * q.Y)) // Pitch 
                )),
                Denormalize(ToDegrees(
                    (float)Math.Atan2(2f * q.X * q.W + 2f * q.Y * q.Z, 1 - 2f * (sqz + sqw))// Yaw 
                )) * -1f + 180f,
                Denormalize(ToDegrees(
                    (float)Math.Atan2(2f * q.X * q.Y + 2f * q.Z * q.W, 1 - 2f * (sqy + sqz))
                ))
            );
        }

        /// <summary>
        ///  欧拉角转四元数
        /// </summary>
        /// <param name="vect"></param>
        /// <returns></returns>
        public static Quaternion ToQuaternion(this Vector3 vect)
        {
            var rollOver2 = ToRadians(Denormalize(vect.Z) - 180f) * 0.5f;
            var sinRollOver2 = (float)Math.Sin((double)rollOver2);
            var cosRollOver2 = (float)Math.Cos((double)rollOver2);
            var pitchOver2 = ToRadians(Denormalize(vect.Y) - 180f) * 0.5f;
            var sinPitchOver2 = (float)Math.Sin((double)pitchOver2);
            var cosPitchOver2 = (float)Math.Cos((double)pitchOver2);
            var yawOver2 = ToRadians(Denormalize(vect.X) * -1f) * 0.5f; // pitch
            var sinYawOver2 = (float)Math.Sin((double)yawOver2);
            var cosYawOver2 = (float)Math.Cos((double)yawOver2);
            return new Quaternion(
                cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2,
                cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2,
                cosYawOver2 * sinPitchOver2 * cosRollOver2 + sinYawOver2 * cosPitchOver2 * sinRollOver2,
                sinYawOver2 * cosPitchOver2 * cosRollOver2 - cosYawOver2 * sinPitchOver2 * sinRollOver2
            );
        }
    }
}
