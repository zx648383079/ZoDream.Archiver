using System;
using UnityEngine;
using UnityEngine.Document;
using ZoDream.BundleExtractor.Unity.Document;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class Texture2DArrayConverter : BundleConverter<Texture2DArray>, IElementTypeLoader
    {
        public object? Read(IBundleBinaryReader reader, Type target, VirtualDocument typeMaps)
        {
            var res = new Texture2DArray();
            var container = reader.Get<ISerializedFile>();
            new DocumentReader(container).Read(typeMaps, reader, res);
            return res;
        }

        public override Texture2DArray? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new Texture2DArray();
            var version = reader.Get<Version>();
            UnityConverter.ReadTexture(res, reader, serializer);
            res.ColorSpace = reader.ReadInt32();
            res.Format = (GraphicsFormat)reader.ReadInt32();
            res.Width = reader.ReadInt32();
            res.Height = reader.ReadInt32();
            res.Depth = reader.ReadInt32();
            res.MipCount = reader.ReadInt32();
            res.DataSize = reader.ReadUInt32();
            res.TextureSettings = serializer.Deserialize<GLTextureSettings>(reader);
            if (version.GreaterThanOrEquals(2020, 2)) //2020.2 and up
            {
                var m_UsageMode = reader.ReadInt32();
            }
            var m_IsReadable = reader.ReadBoolean();
            reader.AlignStream();

            var imageDataSize = reader.ReadInt32();
            if (imageDataSize == 0)
            {
                res.StreamData = serializer.Deserialize<ResourceSource>(reader);
            }

            if (!string.IsNullOrEmpty(res.StreamData.Source))
            {
                var container = reader.Get<ISerializedFile>();
                if (reader.TryGet<IDependencyBuilder>(out var builder))
                {
                    var fileName = container.FullPath;
                    var fileId = reader.Get<ObjectInfo>().FileID;
                    builder.AddDependencyEntry(fileName,
                        fileId,
                        res.StreamData.Source);
                }
                res.ImageData = container.OpenResource(res.StreamData);
            }
            else
            {
                res.ImageData = new PartialStream(reader.BaseStream, imageDataSize);
            }

            res.TextureList = [];
            return res;
        }

        
    }
}
