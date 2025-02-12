using FMOD;
using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Buffers;

namespace ZoDream.FModExporter
{
    public class FModWriter
    {

        public void Write(Stream output, Stream input)
        {
            if (input.Length == 0)
            {
                return;
            }
            var exinfo = new CREATESOUNDEXINFO();
            var result = Factory.System_Create(out var system);
            if (result != RESULT.OK)
            {
                return;
            }
            result = system.init(1, INITFLAGS.NORMAL, IntPtr.Zero);
            if (result != RESULT.OK)
            {
                return;
            }
            exinfo.cbsize = Marshal.SizeOf(exinfo);
            exinfo.length = (uint)input.Length;
            var buffer = ArrayPool<byte>.Shared.Rent((int)input.Length);
            try
            {
                result = system.createSound(buffer, MODE.OPENMEMORY, ref exinfo, out var sound);
                if (result != RESULT.OK)
                {
                    return;
                }
                result = sound.getNumSubSounds(out var numsubsounds);
                if (result != RESULT.OK)
                {
                    return;
                }
                var writer = new BinaryWriter(output);
                if (numsubsounds > 0)
                {
                    result = sound.getSubSound(0, out var subsound);
                    if (result != RESULT.OK)
                    {
                        return;
                    }
                    SoundToWav(subsound, writer);
                    subsound.release();
                }
                else
                {
                    SoundToWav(sound, writer);
                }
                sound.release();
                system.release();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private void SoundToWav(Sound sound, BinaryWriter output)
        {
            var result = sound.getFormat(out _, out _, out int channels, out int bits);
            if (result != RESULT.OK)
            {
                return;
            }
            result = sound.getDefaults(out var frequency, out _);
            if (result != RESULT.OK)
            {
                return;
            }
            var sampleRate = (int)frequency;
            result = sound.getLength(out var length, TIMEUNIT.PCMBYTES);
            if (result != RESULT.OK)
            {
                return;
            }
            result = sound.@lock(0, length, out var ptr1, out var ptr2, out var len1, out var len2);
            if (result != RESULT.OK)
            {
                return;
            }

            //添加 wav 头
            output.Write("RIFF"u8);
            output.Write(len1 + 36);
            output.Write("WAVE"u8);
            output.Write("fmt "u8);
            output.Write(16U);
            output.Write((short)1);
            output.Write((short)channels);
            output.Write(sampleRate);
            output.Write(sampleRate * channels * bits / 8);
            output.Write((short)(channels * bits / 8));
            output.Write((short)bits);
            output.Write("data"u8);
            output.Write(len1);
            var buffer = ArrayPool<byte>.Shared.Rent((int)len1);
            try
            {
                Marshal.Copy(ptr1, buffer, 0, (int)len1);
                result = sound.unlock(ptr1, ptr2, len1, len2);
                output.Write(buffer, 0, (int)len1);
                if (result != RESULT.OK)
                {
                    return;
                }
            } finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
