using System.Text;

namespace ZoDream.Shared.Bundle
{
    public static class ByteExtension
    {
        /// <summary>
        /// 匹配字节
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool StartsWith(this byte[] buffer, byte[] value)
        {
            return IsMatch(buffer, 0, value, 0, value.Length);
        }
        /// <summary>
        /// 匹配字节
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value">ASCII 编码的文字</param>
        /// <returns></returns>
        public static bool StartsWith(this byte[] buffer, string value)
        {
            return StartsWith(buffer, Encoding.ASCII.GetBytes(value));
        }
        /// <summary>
        /// 匹配字节
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value">允许 -1 为不匹配，byte 加上一个 -1 </param>
        /// <returns></returns>
        public static bool StartsWith(this byte[] buffer, int[] value)
        {
            if (buffer.Length < value.Length)
            {
                return false;
            }
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] < 0)
                {
                    continue;
                }
                if (buffer[i] != value[i])
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 查找字符
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int IndexOf(this byte[] buffer, byte[] value)
        {
            return IndexOf(buffer, 0, value);
        }
        /// <summary>
        /// 查找指定字符
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int IndexOf(this byte[] buffer, int offset, byte[] value)
        {
            if (value.Length == 0)
            {
                return -1;
            }
            for (var i = offset; i <= buffer.Length - value.Length; i++)
            {
                if (IsMatch(buffer, i, value, 0, value.Length))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 指定位置是否是字符
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="value"></param>
        /// <param name="valOffset"></param>
        /// <param name="valCount"></param>
        /// <returns></returns>
        public static bool IsMatch(this byte[] buffer, 
            int offset, 
            byte[] value, 
            int valOffset, 
            int valCount)
        {
            if (valCount == 0 
                || valOffset + valCount > value.Length
                ||  offset + valCount > buffer.Length)
            {
                return false;
            }
            for (var i = 0; i < valCount; i++)
            {
                if (buffer[offset + i] != value[valOffset + i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
