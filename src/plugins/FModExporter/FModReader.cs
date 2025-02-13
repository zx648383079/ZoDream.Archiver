using FMOD;
using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Buffers;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;
using System.Threading;
using System.Collections.Generic;
using ZoDream.Shared.Storage;
using System.Text;
using System.Diagnostics;

namespace ZoDream.FModExporter
{
    public class FModReader : IArchiveReader
    {
        public FModReader(Stream input, string fileName, IArchiveOptions? options)
        {
            _fileName = fileName;
            _options = options;
            BaseStream = input;
            NativeMethods.Ready();
            /// 一定要注意 VERSION.number 跟 dll 版本匹配
            var result = Factory.System_Create(out _instance);
            if (result != RESULT.OK)
            {
                throw new ArgumentNullException($"fmod.dll is not found or fmod version is not <{VERSION.number}>");
            }
            result = _instance.init(32, INITFLAGS.NORMAL, IntPtr.Zero);
            if (result != RESULT.OK)
            {
                return;
            }
        }

        private readonly string _fileName;
        private readonly FMOD.System _instance;
        private readonly IArchiveOptions? _options;
        private readonly Stream BaseStream;

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            throw new NotImplementedException();
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            ReadEntry((sound, index, subCount) => {
                if (token.IsCancellationRequested)
                {
                    return false;
                }
                sound.getName(out var name, 100);
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = _fileName;
                }
                var fileName = Path.Combine(folder, Path.GetFileName(name.Replace('/', '\\')));
                if (!LocationStorage.TryCreate(fileName, ".wav", mode, out var fullPath))
                {
                    return true;
                }
                
                SoundToWav(sound, () => {
                    return new BinaryWriter(File.Create(fullPath), 
                        Encoding.ASCII, false);
                });
                return true;
            });
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            var items = new List<IReadOnlyEntry>();
            ReadEntry((sound, index, _) => {
                items.Add(Convert(sound, index));
                return true;
            });
            return items;
        }

        private void ReadEntry(Func<Sound, int, int, bool> cb)
        {
            var length = (int)BaseStream.Length;
            var buffer = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                BaseStream.ReadExactly(buffer, 0, length);
                var info = new CREATESOUNDEXINFO();
                info.cbsize = Marshal.SizeOf(info);
                info.length = (uint)length;
                var result = _instance.createSound(buffer, MODE.ACCURATETIME | MODE.OPENMEMORY, ref info, out var sound);
                if (result != RESULT.OK)
                {
                    return;
                }
                result = sound.getNumSubSounds(out var subSoundCount);
                if (result == RESULT.OK && subSoundCount == 0)
                {
                    cb.Invoke(sound, 0, 0);
                } else
                {
                    for (var i = 0; i < subSoundCount; i++)
                    {
                        result = sound.getSubSound(i, out var subSound);
                        if (result != RESULT.OK)
                        {
                            break;
                        }
                        subSound.seekData(0);
                        var res = cb.Invoke(subSound, i, subSoundCount);
                        subSound.release();
                        if (!res)
                        {
                            break;
                        }
                    }
                }
                sound.release();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private ArchiveEntry Convert(Sound sound, int index)
        {
            sound.getLength(out var length, TIMEUNIT.PCMBYTES);
            sound.getName(out var name, 100);
            return new ArchiveEntry(string.IsNullOrWhiteSpace(name) ? string.Empty : name, index, length);
        }

        private void SoundToWav(Sound sound, Func<BinaryWriter> outputFn)
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
            var buffer = ArrayPool<byte>.Shared.Rent((int)length);
            try
            {
                var readLen = Read(sound, buffer, length);
                if (readLen == 0)
                {
                    return;
                }
                //添加 wav 头
                using var output = outputFn.Invoke();
                output.Write("RIFF"u8);
                output.Write(readLen + 36);
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
                output.Write(readLen);
                output.Write(buffer, 0, (int)readLen);
                output.BaseStream.Flush();
            } finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private uint Read(Sound sound, byte[] buffer, uint length)
        {
            var result = sound.readData(buffer, out var readLen);
            if (result == RESULT.OK)
            {
                // System.Diagnostics.Debug.WriteLine(Error.String(result));
                return readLen;
            }
            result = sound.@lock(0, length, out var ptr1, out var ptr2, out var len1, out var len2);
            if (result != RESULT.OK)
            {
                return 0;
            }
            Marshal.Copy(ptr1, buffer, 0, (int)len1);
            result = sound.unlock(ptr1, ptr2, len1, len2);
            if (result != RESULT.OK)
            {
                return 0;
            }
            return len1;
        }

        public void Dispose()
        {
            _instance.close();
            _instance.release();
            if (_options?.LeaveStreamOpen != true)
            {
                BaseStream.Dispose();
            }
        }

    }
}
