using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Document;
using ZoDream.BundleExtractor.Unity.Document;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class SpriteConverter : BundleConverter<Sprite>, ITypeTreeConverter
    {
        public object? Read(IBundleBinaryReader reader, Type target, VirtualDocument typeMaps)
        {
            var version = reader.Get<Version>();
            var res = new Sprite();
            var container = reader.Get<ISerializedFile>();
            new DocumentReader(container).Read(typeMaps, reader, res);
            if (res.RD?.VertexData is not null)
            {
                VertexDataConverter.GetStreams(res.RD.VertexData, version);
            }
            return res;
        }
        public override Sprite? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            var version = reader.Get<Version>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            var res = new Sprite
            {
                Name = reader.ReadAlignedString(),
                Rect = reader.ReadVector4(),
                Offset = reader.ReadVector2()
            };
            if (version.GreaterThanOrEquals(4, 5)) //4.5 and up
            {
                res.Border = reader.ReadVector4();
            }

            res.PixelsToUnits = reader.ReadSingle();
            if (version.GreaterThanOrEquals(5, 4, 1, VersionType.Patch, 3)) //5.4.1p3 and up
            {
                res.Pivot = reader.ReadVector2();
            }

            res.Extrude = reader.ReadUInt32();
            if (version.GreaterThanOrEquals(5, 3)) //5.3 and up
            {
                res.IsPolygon = reader.ReadBoolean();
                reader.AlignStream();
            }

            if (version.GreaterThanOrEquals(2017)) //2017 and up
            {
                var first = new Guid(reader.ReadBytes(16));
                var second = reader.ReadInt64();
                res.RenderDataKey = new KeyValuePair<Guid, long>(first, second);

                res.AtlasTags = reader.ReadArray(r => r.ReadAlignedString());

                res.SpriteAtlas = reader.ReadPPtr<SpriteAtlas>(serializer);
            }

            res.RD = serializer.Deserialize<SpriteRenderData>(reader);

            if (version.GreaterThanOrEquals(2017)) //2017 and up
            {
                res.PhysicsShape = reader.ReadArray(_ => reader.ReadArray(_ => reader.ReadVector2()));
            }

            //vector m_Bones 2018 and up
            return res;
        }

       
    }
}
