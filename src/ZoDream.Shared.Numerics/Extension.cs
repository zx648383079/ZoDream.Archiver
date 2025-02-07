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
    }
}
