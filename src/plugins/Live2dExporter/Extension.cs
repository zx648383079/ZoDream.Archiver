using System;
using System.IO;
using System.Text;
using ZoDream.Shared.Bundle;

namespace ZoDream.Live2dExporter
{
    internal static class IOExtension
    {
        public static T[] ReadArray<T>(this IBundleBinaryReader reader, uint ptr, int length, Func<T> cb)
        {
            reader.BaseStream.Seek(ptr, SeekOrigin.Begin);
            return reader.ReadArray(length, (_, _) => cb());
        }

        public static string ConvertString(byte[] buffer)
        {
            for (int i = buffer.Length - 1; i >= 0; i--)
            {
                if (buffer[i] != 0x0)
                {
                    return Encoding.UTF8.GetString(buffer, 0, i + 1);
                }
            }
            return string.Empty;
        }
    }
}
