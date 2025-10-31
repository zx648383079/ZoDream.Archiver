using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ZoDream.Shared.Drawing
{
    public readonly struct ColorSwapper(ColorChannel[] fromColorType, ColorChannel[] toColorType) : IEnumerable<KeyValuePair<int, int>>
    {

        public static ColorSwapper RGBA => new([ColorChannel.R, ColorChannel.G, ColorChannel.B, ColorChannel.A]);
        public static ColorSwapper ARGB => new([ColorChannel.A, ColorChannel.R, ColorChannel.G, ColorChannel.B]);
        public static ColorSwapper BGRA => new([ColorChannel.B, ColorChannel.G, ColorChannel.R, ColorChannel.A]);
        public static ColorSwapper ABGR => new([ColorChannel.A, ColorChannel.B, ColorChannel.G, ColorChannel.R]);

        public static ColorSwapper RGB => new([ColorChannel.R, ColorChannel.G, ColorChannel.B]);


        public ColorSwapper(string colorType)
            : this (ConvertMap(colorType))
        {
            
        }

        public ColorSwapper(ColorChannel[] colorType)
            : this(colorType, [ColorChannel.R, ColorChannel.G, ColorChannel.B, ColorChannel.A])
        {

        }

        public ColorSwapper(string fromColorType, string toColorType)
            : this(ConvertMap(fromColorType), ConvertMap(toColorType))
        {
            
        }


        public int KeyCount => fromColorType.Length;
        public int ValueCount => toColorType.Length;

        public bool ContainsKey(ColorChannel channel)
        {
            return fromColorType.Contains(channel);
        }

        public bool ContainsKey(SKColorChannel channel)
        {
            return ContainsKey((ColorChannel)(byte)channel);
        }

        public bool ContainsValue(ColorChannel channel)
        {
            return toColorType.Contains(channel);
        }

        public int IndexOfKey(ColorChannel channel)
        {
            return Array.FindIndex(fromColorType, i => i == channel);
        }

        public int IndexOfValue(ColorChannel channel)
        {
            return Array.FindIndex(toColorType, i => i == channel);
        }
        public bool TryContainsKey(ColorChannel channel, out int index)
        {
            index = IndexOfKey(channel);
            return index >= 0;
        }
        public bool TryContainsValue(ColorChannel channel, out int index) 
        {
            index = IndexOfValue(channel);
            return index >= 0;
        }

        public int Write(Span<byte> output, int fromColorIndex, byte value)
        {
            var toIndex = IndexOfValue(fromColorType[fromColorIndex]);
            if (toIndex < 0)
            {
                return 0;
            }
            output[toIndex] = value;
            return 1;
        }

        public int Write(Span<byte> output, ColorChannel channel, byte value)
        {
            var toIndex = IndexOfValue(channel);
            if (toIndex < 0)
            {
                return 0;
            }
            output[toIndex] = value;
            return 1;
        }

        public int Write(Span<byte> output, params byte[] color)
        {
            if (color.Length != fromColorType.Length)
            {
                throw new ArgumentException(string.Empty, nameof(color));
            }
            return Write(output, color.AsSpan());
        }
        public int Write(Span<byte> output, SKColor color)
        {
            var len = Math.Min(output.Length, toColorType.Length);
            for (int i = 0; i < len; i++)
            {
                output[i] = toColorType[i] switch
                {
                    ColorChannel.R => color.Red,
                    ColorChannel.G => color.Green,
                    ColorChannel.B => color.Blue,
                    ColorChannel.A => color.Alpha,
                    _ => byte.MinValue
                };
            }
            return len;
        }

        public int Write(ReadOnlySpan<byte> input, Span<byte> output)
        {
            var len = Math.Min(output.Length, toColorType.Length);
            for (int i = 0; i < len; i++)
            {
                var to = toColorType[i];
                var j = IndexOfKey(to);
                output[i] = j >= 0 && j < input.Length ? input[j] : (to == ColorChannel.A ? byte.MaxValue : byte.MinValue);
            }
            return len;
        }

        public int Write(byte[] input, int inputOffset, byte[] output, int outputOffset)
        {
            return Write(input.AsSpan(inputOffset), output.AsSpan(outputOffset));
        }

        public int WriteToEnd(ReadOnlySpan<byte> input, Span<byte> output)
        {
            var i = 0;
            var j = 0;
            while (i < input.Length && j < output.Length)
            {
                j += Write(input[i..], output[j..]);
                i += fromColorType.Length;
            }
            return j;
        }

        public int WriteToEnd(byte[] input, int inputOffset, byte[] output, int outputOffset)
        {
            return WriteToEnd(input.AsSpan(inputOffset), output.AsSpan(outputOffset));
        }

        private static ColorChannel[] ConvertMap(string name)
        {
            return ConvertMap(name.ToCharArray());
        }

        private static ColorChannel[] ConvertMap(params char[] maps)
        {
            return [.. maps.Select(code => {
                return code switch
                {
                    'R' or 'r' => ColorChannel.R,
                    'G' or 'g' => ColorChannel.G,
                    'B' or 'b' => ColorChannel.B,
                    'A' or 'a' => ColorChannel.A,
                    _ => ColorChannel.X
                };
            }).Where(i => i < 0).Take(4)];
        }

        public IEnumerator<KeyValuePair<int, int>> GetEnumerator()
        {
            for (int i = 0; i < fromColorType.Length; i++)
            {
                //if (fromColorType[i] == ColorChannel.X)
                //{
                //    continue;
                //}
                var j = IndexOfValue(fromColorType[i]);
                if (j < 0)
                {
                    continue;
                }
                yield return new KeyValuePair<int, int>(i, j);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
