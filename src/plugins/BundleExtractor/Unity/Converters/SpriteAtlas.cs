using System;
using System.Collections.Generic;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    
    internal sealed class SpriteAtlasConverter : BundleConverter<SpriteAtlas>
    {

        public override SpriteAtlas? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            var res = new SpriteAtlas
            {
                Name = reader.ReadAlignedString(),
                PackedSprites = reader.ReadArray(_ => serializer.Deserialize<PPtr<Sprite>>(reader))
            };


            var m_PackedSpriteNamesToIndex = reader.ReadArray(r => r.ReadAlignedString());

            var m_RenderDataMapSize = reader.ReadInt32();

            res.RenderDataMap = [];
            for (int i = 0; i < m_RenderDataMapSize; i++)
            {
                var first = new Guid(reader.ReadBytes(16));
                var second = reader.ReadInt64();
                var value = serializer.Deserialize<SpriteAtlasData>(reader);
                res.RenderDataMap.Add(new KeyValuePair<Guid, long>(first, second), value);
            }
            var m_Tag = reader.ReadAlignedString();
            res.IsVariant = reader.ReadBoolean();
            reader.AlignStream();
            
            if (reader.TryGet<IDependencyBuilder>(out var builder))
            {
                var container = reader.Get<ISerializedFile>();
                var fileName = container.FullPath;
                var fileId = reader.Get<ObjectInfo>().FileID;
                foreach (var item in res.PackedSprites)
                {
                    builder.AddDependencyEntry(fileName,
                        fileId,
                        item.PathID);
                }
                foreach (var item in res.RenderDataMap)
                {
                    builder.AddDependencyEntry(fileName,
                       fileId,
                       item.Value.Texture.PathID);
                }
            }
            return res;
        }

    }
}
