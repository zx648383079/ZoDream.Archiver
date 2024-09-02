using System;
using System.IO;
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
    }
}
