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
using Version = System.Version;

namespace ZoDream.BundleExtractor.Xna
{
    public class XnbReader(IBundleBinaryReader reader, IEntryService service, IBundleOptions options) : IBundleHandler
    {
        private static readonly string Signature = "XNB";
        private BundleCodecType _codecType = BundleCodecType.Unknown;

        private readonly IBundleSerializer _serializer = service.Get<IBundleSerializer>();
        private KeyValuePair<string, Version>[] _typeItems = [];

        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
        {
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
            // var sharedCount = nextReader.Read7BitEncodedInt();
            var entryBeginTag = nextReader.Read7BitEncodedInt();
            Expectation.ThrowIf(entryBeginTag != 0, $"Tag: {entryBeginTag} != 0");
            nextReader.Add(this);
            while (nextReader.RemainingLength > 0)
            {
                entryBeginTag = nextReader.Read7BitEncodedInt();
                var entry = ReadObject(nextReader, entryBeginTag - 1);
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
                case "Microsoft.Xna.Framework.Content.DictionaryReader":
                    return ReadDictionary(reader, args[1], args[2]);
                case "Microsoft.Xna.Framework.Content.EnumReader":
                    return ReadObject(reader, args[1]);
                // Array
                case "Microsoft.Xna.Framework.Content.ArrayReader":
                case "Microsoft.Xna.Framework.Content.ListReader":
                case "System.Collections.Generic.List":
                case "Array":
                    return ReadArray(reader, args[1]);
            }
            if (!unknowTag || IsValueType(args[0]))
            {
                return _serializer.Deserialize(reader, ToType(args[0]));
            }
            var entryBeginTag = reader.Read7BitEncodedInt();
            if (entryBeginTag == 0)
            {
                return null;
            }
            return ReadObject(reader, entryBeginTag - 1);
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
            return type switch
            {
                "Microsoft.Xna.Framework.Content.BooleanReader" or "System.Boolean" => typeof(bool),
                // Char
                "Microsoft.Xna.Framework.Content.CharReader" or "System.Char" => typeof(char),
                // Int32
                "Microsoft.Xna.Framework.Content.Int32Reader" or "System.Int32" => typeof(int),
                "Microsoft.Xna.Framework.Content.Int16Reader" or "System.Int16" => typeof(short),
                "Microsoft.Xna.Framework.Content.Int64Reader"
                or "Microsoft.Xna.Framework.Content.TimeSpanReader" 
                or "System.Int64" => typeof(long),
                "Microsoft.Xna.Framework.Content.SByteReader" or "System.SByte" => typeof(sbyte),
                "Microsoft.Xna.Framework.Content.ByteReader" or "System.Byte" => typeof(byte),
                "Microsoft.Xna.Framework.Content.SingleReader" or "System.Single" => typeof(float),
                "Microsoft.Xna.Framework.Content.UInt16Reader" or "System.UInt16" => typeof(ushort),
                "Microsoft.Xna.Framework.Content.UInt32Reader" or "System.UInt32" => typeof(uint),
                "Microsoft.Xna.Framework.Content.UInt64Reader" or "System.UInt64" => typeof(ulong),
                // String
                "Microsoft.Xna.Framework.Content.StringReader" or "System.String" => typeof(string),
                // Texture2D
                "Microsoft.Xna.Framework.Content.Texture2DReader" => typeof(Texture2D),
                "Microsoft.Xna.Framework.Content.Texture3DReader" => typeof(Texture3D),
                "Microsoft.Xna.Framework.Content.TextureCubeReader" => typeof(TextureCube),
                "Microsoft.Xna.Framework.Content.BoundingBoxReader" => typeof(MinMaxAABB),
                "Microsoft.Xna.Framework.Content.ColorReader" => typeof(Color),
                "Microsoft.Xna.Framework.Content.BoundingFrustumReader" 
                or "Microsoft.Xna.Framework.Content.MatrixReader" => typeof(Matrix4x4),
                "Microsoft.Xna.Framework.Content.BoundingSphereReader" => typeof(Vector4),
                "Microsoft.Xna.Framework.Content.CurveReader" => typeof(FloatCurve),
                "Microsoft.Xna.Framework.Content.DateTimeReader" => typeof(DateTime),
                "Microsoft.Xna.Framework.Content.DecimalConverter" => typeof(decimal),
                "Microsoft.Xna.Framework.Content.DoubleReader" => typeof(double),
                "Microsoft.Xna.Framework.Content.IndexBufferReader" => typeof(IndexBuffer),
                // Vector2
                "Microsoft.Xna.Framework.Content.Vector2Reader" or "Microsoft.Xna.Framework.Vector2" => typeof(Vector2),
                // Vector3
                "Microsoft.Xna.Framework.Content.Vector3Reader" or "Microsoft.Xna.Framework.Vector3" => typeof(Vector3),
                // Vector3
                "Microsoft.Xna.Framework.Content.Vector4Reader" 
                or "Microsoft.Xna.Framework.Content.PlaneReader"
                or "Microsoft.Xna.Framework.Content.QuaternionReader"
                or "Microsoft.Xna.Framework.Vector4" => typeof(Vector4),
                // SpriteFont
                "Microsoft.Xna.Framework.Content.SpriteFontReader" => typeof(SpriteFont),
                "Microsoft.Xna.Framework.Content.RayReader" => typeof(Ray),
                "Microsoft.Xna.Framework.Content.VertexBufferReader" => typeof(VertexBuffer),
                "Microsoft.Xna.Framework.Content.VertexDeclarationReader" => typeof(VertexDeclaration),
                "Microsoft.Xna.Framework.Content.VideoReader" => typeof(VideoClip),

                "Microsoft.Xna.Framework.Content.SkinnedEffectReader" => typeof(SkinnedEffect),
                "Microsoft.Xna.Framework.Content.EffectMaterialReader" => typeof(EffectMaterial),
                "Microsoft.Xna.Framework.Content.DualTextureEffectReader" => typeof(DualTextureEffect),
                // Rectangle
                "Microsoft.Xna.Framework.Content.PointReader" => typeof(Vector2Int),
                "Microsoft.Xna.Framework.Content.RectangleReader" or "Microsoft.Xna.Framework.Rectangle" => typeof(Vector4Int),
                // Effect
                "Microsoft.Xna.Framework.Content.EffectReader" or "Microsoft.Xna.Framework.Graphics.Effect" => typeof(Effect),
                // xTile TBin
                "xTile.Pipeline.TideReader" => typeof(Stream),
                "BmFont.XmlSourceReader" => typeof(string),
                "Microsoft.Xna.Framework.Content.SoundEffectReader" or "Microsoft.Xna.Framework.SoundEffect" => typeof(SoundEffect),
                "Microsoft.Xna.Framework.Content.SongReader" => typeof(Song),
                _ => throw new Shared.NotSupportedException(type),
            };
        }

        private bool IsValueType(string type)
        {
            return type switch
            {
                "Microsoft.Xna.Framework.Content.BooleanReader" or "System.Boolean" 
                or "Microsoft.Xna.Framework.Content.CharReader" or "System.Char"
                or "Microsoft.Xna.Framework.Content.Int32Reader" or "System.Int32"
                or "Microsoft.Xna.Framework.Content.Int16Reader" or "System.Int16"
                or "Microsoft.Xna.Framework.Content.Int64Reader"
                or "Microsoft.Xna.Framework.Content.TimeSpanReader"
                or "System.Int64"
                or "Microsoft.Xna.Framework.Content.SByteReader" or "System.SByte"
                or "Microsoft.Xna.Framework.Content.ByteReader" or "System.Byte"
                or "Microsoft.Xna.Framework.Content.SingleReader" or "System.Single"
                or "Microsoft.Xna.Framework.Content.UInt16Reader" or "System.UInt16"
                or "Microsoft.Xna.Framework.Content.UInt32Reader" or "System.UInt32"
                or "Microsoft.Xna.Framework.Content.UInt64Reader" or "System.UInt64"
                or "Microsoft.Xna.Framework.Content.StringReader" or "System.String" => true,
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
                return [type];
            }
            var res = new List<string>(3)
            {
                type[..i]
            };
            foreach (Match item in Regex.Matches(type[i..], @"\[([^\[\]]+)\]"))
            {
                res.Add(item.Groups[1].Value.Split(',', 2)[0]);
            }
            return [..res];
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
