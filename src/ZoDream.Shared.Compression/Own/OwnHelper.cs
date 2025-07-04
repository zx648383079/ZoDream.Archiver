using SharpCompress.Compressors.Xz;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ZoDream.Shared.Compression.Own
{
    public static class OwnHelper
    {
        public static string RandomName(params string[] items)
        {
            return "z_" + MD5Encode(string.Join(',', items) + DateTime.Now.ToLongTimeString());
        }

        public static string MD5Encode(string source)
        {
            var sor = Encoding.UTF8.GetBytes(source);
            var result = MD5.HashData(sor);
            return Convert.ToHexString(result).ToLower();
        }

        public static string GetSafePath(string fileName)
        {
            foreach (var item in Path.GetInvalidPathChars())
            {
                fileName = fileName.Replace(item, '_');
            }
            return fileName;
        }

        /// <summary>
        /// 在指定范围0-max内容循环
        /// </summary>
        /// <param name="val"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Clamp(int val, int max)
        {
            var res = val % max;
            return res >= 0 ? res : (res + max);
        }


        public static long ReadLength(Stream input)
        {
            var code = input.ReadByte();
            if (code <= 250)
            {
                return code;
            }
            if (code <= 252)
            {
                return input.ReadByte() + code * (code - 250);
            }
            var len = code - 251;
            var buffer = new byte[len];
            input.ReadExactly(buffer, 0, len);
            var res = 0L;
            for (var j = len - 2; j >= 0; j--)
            {
                res += (long)Math.Pow(code, j);
            }
            for (var i = 0; i < len; i++)
            {
                res += (long)(buffer[i] * Math.Pow(256, len - i - 1));
            }
            return res;
        }

        public static long ReadLength(byte[] buffer, out int usedCount)
        {
            return ReadLength(buffer, 0, out usedCount);
        }

        public static long ReadLength(byte[] buffer, int offset, out int usedCount)
        {
            var code = buffer[offset++];
            usedCount = 1;
            if (code <= 250)
            {
                return code;
            }
            if (code <= 252)
            {
                usedCount++;
                return buffer[offset] + code * (code - 250);
            }
            var len = code - 251;
            var res = 0L;
            for (var j = len - 2; j >= 0; j--)
            {
                res += (long)Math.Pow(code, j);
            }
            for (var i = 0; i < len; i++)
            {
                res += (long)(buffer[i + offset] * Math.Pow(256, len - i - 1));
            }
            usedCount += len;
            return res;
        }


        public static void WriteLength(Stream input, long length)
        {
            input.Write(WriteLength(length));
        }

        public static int WriteLength(long length, byte[] input, int offset)
        {
            if (length <= 250)
            {
                input[offset] = (byte)length;
                return 1;
            }

            var basic = 250;
            int i;
            // 相加
            for (i = 251; i <= 252; i++)
            {
                var plus = i * (i - basic);
                if (length <= plus + 255)
                {
                    input[offset] = (byte)i;
                    input[offset + 1] = (byte)(length - plus);
                    return 2;
                }
            }
            // 倍数
            basic = 252;
            i = 253;
            for (; i <= 255; i++)
            {
                var len = i - basic + 1;
                input[offset] = (byte)i;
                var b = 0L;
                for (var j = len - 2; j >= 0; j--)
                {
                    b += (long)Math.Pow(i, j);
                }
                var rate = length - b;
                for (var j = 0; j < len; j++)
                {
                    input[offset + len - j] = (byte)(rate % 256);
                    rate /= 256;
                }
                if (rate == 0)
                {
                    return len + 1;
                }
            }
            throw new ArgumentOutOfRangeException();
        }

        public static byte[] WriteLength(long length)
        {
            if (length <= 250)
            {
                return [(byte)length];
            }

            var basic = 250;
            int i;
            // 相加
            for (i = 251; i <= 252; i++)
            {
                var plus = i * (i - basic);
                if (length <= plus + 255)
                {
                    return [(byte)i, (byte)(length - plus)];
                }
            }
            // 倍数
            basic = 252;
            i = 253;
            for (; i <= 255; i++)
            {
                var len = i - basic + 1;
                var buffer = new byte[len + 1];
                buffer[len] = (byte)i;
                var b = 0L;
                for (var j = len - 2; j >= 0; j--)
                {
                    b += (long)Math.Pow(i, j);
                }
                var rate = length - b;
                for (var j = 0; j < len; j++)
                {
                    buffer[j] = (byte)(rate % 256);
                    rate /= 256;
                }
                if (rate == 0)
                {
                    return buffer.Reverse().ToArray();
                }
            }
            throw new ArgumentOutOfRangeException();
        }

    }
}
