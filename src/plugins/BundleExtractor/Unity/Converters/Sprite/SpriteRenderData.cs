using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SpriteRenderDataConverter : BundleConverter<SpriteRenderData>
    {
        public override SpriteRenderData? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new SpriteRenderData();
            res.Texture = reader.ReadPPtr<Texture2D>(serializer);
            if (version.GreaterThanOrEquals(5, 2)) //5.2 and up
            {
                res.AlphaTexture = reader.ReadPPtr<Texture2D>(serializer);
            }

            if (version.GreaterThanOrEquals(2019)) //2019 and up
            {
                res.SecondaryTextures = reader.ReadArray(_ => serializer.Deserialize<SecondarySpriteTexture>(reader));

            }

            if (version.GreaterThanOrEquals(5, 6)) //5.6 and up
            {
                var subMeshCount = reader.ReadInt32();
                if (subMeshCount > short.MaxValue)
                {
                    throw new OutOfMemoryException("SpriteRender SubMesh Count too large");
                }
                res.SubMeshes = reader.ReadArray(subMeshCount, _ => serializer.Deserialize<SubMesh>(reader));

                res.IndexBuffer = reader.ReadArray(r => r.ReadByte());
                reader.AlignStream();

                res.VertexData = serializer.Deserialize<VertexData>(reader);
            }
            else
            {
                res.Vertices = reader.ReadArray(_ => serializer.Deserialize<SpriteVertex>(reader));


                res.Indices = reader.ReadArray(r => r.ReadUInt16());
                reader.AlignStream();
            }

            if (version.GreaterThanOrEquals(2018)) //2018 and up
            {
                res.Bindpose = reader.ReadMatrixArray();

                if (version.LessThan(2018, 2)) //2018.2 down
                {
                    res.SourceSkin = reader.ReadArray(_ => serializer.Deserialize<BoneWeights4>(reader));
                }
            }

            res.TextureRect = reader.ReadVector4();
            res.TextureRectOffset = reader.ReadVector2();
            if (version.GreaterThanOrEquals(5, 6)) //5.6 and up
            {
                res.AtlasRectOffset = reader.ReadVector2();
            }

            res.SettingsRaw = serializer.Deserialize<SpriteSettings>(reader);
            if (version.GreaterThanOrEquals(4, 5)) //4.5 and up
            {
                res.UvTransform = reader.ReadVector4();
            }

            if (version.GreaterThanOrEquals(2017)) //2017 and up
            {
                res.DownscaleMultiplier = reader.ReadSingle();
            }
            return res;
        }
    }

}
