using System;
using UnityEngine;
using UnityEngine.Document;
using ZoDream.BundleExtractor.Unity.Document;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    public class AssetBundleManifestConverter : BundleConverter<AssetBundleManifest>, IElementTypeLoader
    {
        public object? Read(IBundleBinaryReader reader, Type target, VirtualDocument typeMaps)
        {
            var res = new AssetBundleManifest();
            var container = reader.Get<ISerializedFile>();
            new DocumentReader(container).Read(typeMaps, reader, res);
            return res;
        }

        public override AssetBundleManifest? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new AssetBundleManifest();


            var container = reader.Get<ISerializedFile>();
            if (reader.TryGet<IDependencyBuilder>(out var builder))
            {
                for (int i = 0; i < res.AssetBundleInfos.Length; i++)
                {
                    var entry = res.AssetBundleInfos[i];
                    foreach (var item in entry.AssetBundleDependencies)
                    {
                        builder.AddDependency(
                            res.AssetBundleNames[i],
                            res.AssetBundleNames[item]
                        );
                    }
                }
            }
            return res;
        }
    }
}
