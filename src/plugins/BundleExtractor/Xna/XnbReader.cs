using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Numerics;
using ZoDream.Shared.Storage;
using Version = System.Version;

namespace ZoDream.BundleExtractor.Xna
{
    public class XnbReader(IBundleBinaryReader reader, string fileName, IEntryService service, IBundleOptions options) : IBundleHandler
    {
        private static readonly string Signature = "XNB";
        private BundleCodecType _codecType = BundleCodecType.Unknown;

        private readonly IBundleSerializer _serializer = service.Get<IBundleSerializer>();
        private KeyValuePair<string, Version>[] _typeItems = [];
        private ArchiveExtractMode _exportMode;
        private string _exportPath;

        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
        {
            _exportMode = mode;
            _exportPath = folder.EndsWith(fileName) ? folder : Path.Combine(folder, fileName);
            if (_exportPath.EndsWith(".xnb"))
            {
                _exportPath = _exportPath[..^4];
            }
            if (!ReadHeader())
            {
                return;
            }
            var length = reader.ReadUInt32();
            Expectation.ThrowIfNot(length == reader.Length, $"Length: {length} != {reader.Length}");
            Stream next = new PartialStream(reader.BaseStream);
            if (_codecType > BundleCodecType.Unknown)
            {
                var uncompressedSize = reader.ReadUInt32();
                next = BundleCodec.Decode(new PartialStream(reader.BaseStream), _codecType, 
                    uncompressedSize);
            }
            using var nextReader = new BundleBinaryReader(next, reader, _codecType == BundleCodecType.Unknown);
            var entryCount = nextReader.Read7BitEncodedInt();
            _typeItems = new KeyValuePair<string, Version>[entryCount];
            for (var i = 0; i < entryCount; i++)
            {
                var type = nextReader.Read7BitEncodedString();
                var version = nextReader.ReadUInt32();
                _typeItems[i] = new KeyValuePair<string, Version>(type, new Version((int)version, 0));
            }
            var entryBeginTag = nextReader.Read7BitEncodedInt();
            if (entryBeginTag != 0)
            {
                // var sharedCount = entryBeginTag;
                entryBeginTag = nextReader.Read7BitEncodedInt();
            } 
            Expectation.ThrowIf(entryBeginTag != 0, $"Tag: {entryBeginTag} != 0");
            nextReader.Add(this);
            while (nextReader.RemainingLength > 0)
            {
                entryBeginTag = nextReader.Read7BitEncodedInt();
                ReadObject(nextReader, entryBeginTag - 1);
            }
        }

        internal object? ReadObject(IBundleBinaryReader reader, int index)
        {
            var typeInfo = _typeItems[index];
            reader.Add(typeInfo.Value);
            return ReadObject(reader, typeInfo.Key);
        }

        internal object? ReadObject(IBundleBinaryReader reader, string type, bool unknowTag = false)
        {
            var args = SplitType(type);
            switch (args[0])
            {
                // Dictionary
                case "Dictionary":
                    return ReadDictionary(reader, args[1], args[2]);
                case "Enum":
                    return ReadObject(reader, args[1]);
                // Array
                case "Array":
                case "List":
                    return ReadArray(reader, args[1]);
            }
            if (!unknowTag || IsValueType(args[0]))
            {
                return Deserialize(reader, ToType(args[0], type), args[0]);
            }
            var entryBeginTag = reader.Read7BitEncodedInt();
            if (entryBeginTag == 0)
            {
                return null;
            }
            return ReadObject(reader, entryBeginTag - 1);
        }

        private object? Deserialize(IBundleBinaryReader reader, Type objectType, string sourceType)
        {
            if (!_serializer.Converters.TryGet(objectType, out var converter))
            {
                return null;
            }
            var res = converter.Read(reader, objectType, _serializer);
            if (res is null)
            {
                return res;
            }
            if (res is string s)
            {
                SaveAs(s, sourceType);
            } else if (converter is IBundleConvertExporter exporter)
            {
                exporter.SaveAs(res, _exportPath, _exportMode);
            }
            return res;
        }

        private void SaveAs(string instance, string sourceType)
        {
            if (string.IsNullOrWhiteSpace(instance))
            {
                return;
            }
            if (!LocationStorage.TryCreate(_exportPath, sourceType.ToLower(), _exportMode, out var outputPath))
            {
                return;
            }
            File.WriteAllText(outputPath, instance);
        }

        internal object? ReadArray(IBundleBinaryReader reader, string type)
        {
            return reader.ReadArray(_ => ReadObject(reader, type, true));
        }

        internal object? ReadDictionary(IBundleBinaryReader reader, string keyType, string valueType)
        {
            return reader.ReadArray(_ => new KeyValuePair<object, object>(ReadObject(reader, keyType, true), ReadObject(reader, valueType, true)));
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
        private static Type ToType(string type)
        {
            return ToType(type, type);
        }
        private static Type ToType(string type, string sourceType)
        {
            return type switch
            {
                "Boolean" => typeof(bool),
                // Char
                "Char" => typeof(char),
                // Int32
                "Int32" => typeof(int),
                "Int16" => typeof(short),
                "TimeSpan" or "Int64" => typeof(long),
                "SByte" => typeof(sbyte),
                "Byte" => typeof(byte),
                "Single" => typeof(float),
                "UInt16" => typeof(ushort),
                "UInt32" => typeof(uint),
                "UInt64" => typeof(ulong),
                // String
                "String" or "XmlSource" or "Lua" or "Anim" or "Tx" 
                or "Tsx" or "Tmx" or "Cmx" => typeof(string),
                // Texture2D
                "Texture2D" => typeof(Texture2D),
                "Texture3D" => typeof(Texture3D),
                "TextureCube" => typeof(TextureCube),
                "BoundingBox" => typeof(MinMaxAABB),
                "Color" => typeof(Color),
                "BoundingFrustum" 
                or "Matrix" => typeof(Matrix4x4),
                "BoundingSphere" => typeof(Vector4),
                "Curve" => typeof(FloatCurve),
                "DateTime" => typeof(DateTime),
                "Decimal" => typeof(decimal),
                "Double" => typeof(double),
                "IndexBuffer" => typeof(IndexBuffer),
                // Vector2
                "Vector2" => typeof(Vector2),
                // Vector3
                "Vector3" => typeof(Vector3),
                // Vector3
                "Vector4" 
                or "Plane" => typeof(Vector4),
                "Quaternion" => typeof(Quaternion),
                // SpriteFont
                "SpriteFont" => typeof(SpriteFont),
                "Ray" => typeof(Ray),
                "VertexBuffer" => typeof(VertexBuffer),
                "VertexDeclaration" => typeof(VertexDeclaration),
                "Video" => typeof(VideoClip),

                "SkinnedEffect" => typeof(SkinnedEffect),
                "EffectMaterial" => typeof(EffectMaterial),
                "DualTextureEffect" => typeof(DualTextureEffect),
                // Rectangle
                "Point" => typeof(Vector2Int),
                "Rectangle" => typeof(Vector4Int),
                // Effect
                "Effect" => typeof(Effect),
                // xTile TBin
                "Tide" => typeof(Stream),
                "SoundEffect" => typeof(SoundEffect),
                "Song" => typeof(Song),
                _ => throw new Shared.NotSupportedException(sourceType),
            };
        }

        private static bool IsValueType(string type)
        {
            return type switch
            {
                "Boolean" 
                or "Char"
                or "Int32"
                or "Int16"
                or "Int64"
                or "TimeSpan"
                or "SByte"
                or "Byte"
                or "Single"
                or "UInt16"
                or "UInt32"
                or "UInt64"
                or "String" => true,
                _ => false,
            };
        }
        public void Dispose()
        {
            reader.Dispose();
        }

        

        private static string[] SplitType(string type)
        {
            var i = type.IndexOf('`');
            if (i < 0)
            {
                if (type.EndsWith("[]"))
                {
                    return ["Array", type[..^2]];
                }
                return [ShortType(type)];
            }
            var res = new List<string>(3)
            {
                ShortType(type[..i])
            };
            foreach (Match item in Regex.Matches(type[i..], @"\[([^\[\]]+)\]"))
            {
                res.Add(item.Groups[1].Value);
            }
            return [..res];
        }

        private static string ShortType(string type)
        {
            var i = type.IndexOf(',');
            if (i > 0)
            {
                type = type[..i];
            }
            i = type.LastIndexOf('.');
            if (i >= 0)
            {
                type = type[(i + 1)..];
            }
            if (type.EndsWith("Reader"))
            {
                return type[..^6];
            }
            return type;
        }

        internal static Vector2Int ReadVector2I(IBundleBinaryReader reader)
        {
            return new Vector2Int(reader.ReadInt32(), reader.ReadInt32());
        }

        internal static Vector4Int ReadVector4I(IBundleBinaryReader reader)
        {
            return new Vector4Int(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        }

        internal static string ReadString(IBundleBinaryReader reader)
        {
            return reader.Read7BitEncodedString();
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
