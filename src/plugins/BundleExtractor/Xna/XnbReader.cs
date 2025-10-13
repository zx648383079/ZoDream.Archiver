using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using ZoDream.Shared;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Numerics;

namespace ZoDream.BundleExtractor.Xna
{
    public class XnbReader(IBundleBinaryReader reader, IEntryService service, IBundleOptions options) : IBundleHandler
    {
        private static readonly string Signature = "XNB";
        private BundleCodecType _codecType = BundleCodecType.Unknown;

        private IBundleSerializer _serializer = service.Get<IBundleSerializer>();
        private KeyValuePair<string, Version>[] _typeItems = [];

        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
        {
            if (!ReadHeader())
            {
                return;
            }
            var length = reader.ReadUInt32();
            Expectation.ThrowIfNot(length != reader.Length);
            Stream next = new PartialStream(reader.BaseStream);
            if (_codecType > BundleCodecType.Unknown)
            {
                var uncompressedSize = reader.ReadUInt32();
                next = BundleCodec.Decode(new PartialStream(reader.BaseStream), _codecType, uncompressedSize);
            }
            using var nextReader = new BundleBinaryReader(next, reader, _codecType == BundleCodecType.Unknown);
            var entryCount = nextReader.Read7BitEncodedInt();
            _typeItems = new KeyValuePair<string, Version>[entryCount];
            for (var i = 0; i < entryCount; i++)
            {
                var type = reader.Read7BitEncodedString();
                var version = reader.ReadUInt32();
                _typeItems[i] = new KeyValuePair<string, Version>(type, new Version((int)version, 0));
            }
            var entryBeginTag = nextReader.Read7BitEncodedInt();
            Expectation.ThrowIf(entryBeginTag != 0);
            reader.Add(this);
            while (reader.RemainingLength > 0)
            {
                entryBeginTag = nextReader.Read7BitEncodedInt();
                var entry = ReadObject(entryBeginTag - 1);
            }
        }

        public object? ReadObject(int index)
        {
            var typeInfo = _typeItems[index];
            reader.Add(typeInfo.Value);
            return _serializer.Deserialize(reader, Parse(typeInfo.Key));
        }

        private bool ReadHeader()
        {
            reader.Position = 0;
            if (!reader.ReadBytes(Signature.Length).Equal(Signature))
            {
                return false;
            }
            var platform = (PlatformTargetType)reader.ReadByte();
            var format = reader.ReadByte();
            Expectation.ThrowIfNot(format - 3 <= 2);
            var compressionType = reader.ReadByte();
            if ((compressionType & 0x40) != 0) 
            {
                _codecType = BundleCodecType.Lz4;
            } else if ((compressionType & 0x80) != 0)
            {
                _codecType = BundleCodecType.Lzx;
            }
            return true;
        }

        private static Type Parse(string type)
        {
            return typeof(bool);
        }


        public void Dispose()
        {
            reader.Dispose();
        }

        internal static Vector2Int ReadVector2I(IBundleBinaryReader reader)
        {
            return new Vector2Int(reader.ReadInt32(), reader.ReadInt32());
        }

        internal static Vector4Int ReadVector4I(IBundleBinaryReader reader)
        {
            return new Vector4Int(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        }

        internal static char ReadChar(IBundleBinaryReader reader)
        {
            var b = reader.ReadByte();
            var count = ((-452984832 >>> ((b >> 3) & 0x1E)) & 3) + 1;
            var buffer = new byte[count];
            buffer[0] = b;
            reader.Read(buffer, 1, count - 1);
            return Encoding.UTF8.GetChars(buffer)[0];
        }
    }
}
