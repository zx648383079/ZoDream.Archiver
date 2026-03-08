using System;
using System.IO;
using System.Text;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class SoundEffectConverter : BundleConverter<SoundEffect>, IBundleConvertExporter<SoundEffect>
    {
        public override SoundEffect? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var count = reader.ReadInt32();
            Expectation.ThrowIfNot(count == 18);
            return new SoundEffect()
            {
                Header = reader.ReadBytes(count),
                Data = reader.ReadAsStream(),
                LoopStart = reader.ReadInt32(),
                LoopLength = reader.ReadInt32(),
                Duration = reader.ReadInt32(),// ms
            };
        }

        public void SaveAs(SoundEffect instance, string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, ".wav", mode, out fileName))
            {
                return;
            }
            using var writer = new BinaryWriter(File.Create(fileName), Encoding.ASCII, false);
            writer.Write("RIFF"u8);
            writer.Write((uint)(20 + instance.Header.Length + instance.Data.Length));
            writer.Write("WAVE"u8);
            writer.Write("fmt "u8);
            writer.Write((uint)instance.Header.Length);
            writer.Write(instance.Header);
            writer.Write("data"u8);
            writer.Write((uint)instance.Data.Length);
            instance.Data.CopyTo(writer.BaseStream);
            writer.BaseStream.Flush();
        }
    }
}
