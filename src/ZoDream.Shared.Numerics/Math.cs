namespace ZoDream.Shared.Numerics
{
    public static class MathEx
    {
        /// <summary>
        /// 求 val / divisor 向上取整
        /// </summary>
        /// <param name="val"></param>
        /// <param name="divisor">被除</param>
        /// <returns></returns>
        public static int Ceiling(int val, int divisor)
        {
            return val / divisor + (val % divisor == 0 ? 0 : 1);
        }
    }
}
