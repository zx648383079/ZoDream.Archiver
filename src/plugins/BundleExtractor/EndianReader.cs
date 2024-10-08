﻿using System;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor
{
    public static class EndianReaderExtension
    {
        public static int[] ReadInt32Array(this EndianReader reader)
        {
            return reader.ReadInt32Array(allowAlignment: true);
        }

        public static int[] ReadInt32Array(this EndianReader reader, bool allowAlignment)
        {
            int num = reader.ReadInt32();
            int i = 0;
            //ThrowIfNotEnoughSpaceForArray(num, 4);
            int[] array = ((num == 0) ? [] : new int[num]);
            for (; i < num; i = checked(i + 1))
            {
                try
                {
                    array[i] = reader.ReadInt32();
                }
                catch (Exception innerException)
                {
                    throw new Exception($"End of stream. Read {i}, expected {num} elements", innerException);
                }
            }

            if (allowAlignment && reader.IsAlignArray)
            {
                reader.AlignStream();
            }

            return array;
        }


        public static T[][] ReadArrayArray<T>(this EndianReader reader,
            Func<EndianReader, T> cb)
        {
            var num = reader.ReadInt32();
            T[][] array = ((num == 0) ? [] : new T[num][]);
            for (int i = 0; i < num; i = checked(i + 1))
            {
                T[] array2 = reader.ReadArray<T>(cb);
                array[i] = array2;
            }

            if (reader.IsAlignArray)
            {
                reader.AlignStream();
            }

            return array;
        }


        public static T[] ReadArray<T>(this EndianReader reader, Func<EndianReader, T> cb)
        {
            return reader.ReadArray(reader.ReadInt32(), cb);
        }

        public static T[] ReadArray<T>(this EndianReader reader, int length, Func<EndianReader, T> cb)
        {
            var array = new T[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = cb.Invoke(reader);
            }
            if (reader.IsAlignArray)
            {
                reader.AlignStream();
            }
            return array;
        }

        public static void AlignStream(this EndianReader reader)
        {
            reader.BaseStream.Position = checked(reader.BaseStream.Position + 3) & -4;
        }

        public static void AlignStream(this EndianReader reader, int alignment)
        {
            var pos = reader.BaseStream.Position;
            var mod = pos % alignment;
            if (mod != 0)
            {
                reader.BaseStream.Seek(alignment - mod, System.IO.SeekOrigin.Current);
            }
        }
    }
}
