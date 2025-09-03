using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace ZoDream.Shared.Drawing
{
    internal ref struct BitStream
    {
        private UInt128 _value;

        private readonly uint BottomBits {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => unchecked((uint)_value);
        }

        public BitStream(UInt128 value)
        {
            _value = value;
        }

        public BitStream(ReadOnlySpan<byte> span) : this(BinaryPrimitives.ReadUInt128LittleEndian(span))
        {
        }

        public BitStream(ulong low, ulong high)
        {
            _value = new UInt128(high, low);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public readonly uint PeakBits(int numBits)
        {
            uint mask = (1u << numBits) - 1u;
            uint bits = BottomBits & mask;
            return bits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Advance(int numBits)
        {
            _value >>= numBits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public uint ReadBits(int numBits)
        {
            uint bits = PeakBits(numBits);
            Advance(numBits);
            return bits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public uint ReadBit()
        {
            uint result = BottomBits & 1u;
            Advance(1);
            return result;
        }

        /// <summary>
        /// Reversed bits pulling, used in BC6H decoding
        /// </summary>
        /// <param name="numBits"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public uint ReadBitsReversed(int numBits)
        {
            uint bits = ReadBits(numBits);
            // Reverse the bits.
            return ColorConverter.ReverseBits(bits) >> 32 - numBits;
        }
    }
}
