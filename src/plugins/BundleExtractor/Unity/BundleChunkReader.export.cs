using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using ZoDream.BundleExtractor.Unity;
using ZoDream.BundleExtractor.Unity.Exporters;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor
{
    internal partial class UnityBundleChunkReader
    {

        private readonly Dictionary<NativeClassID, Type> _exportItems = new() 
        {
            {NativeClassID.TextAsset, typeof(RawExporter)},
            {NativeClassID.AudioClip, typeof(FsbExporter)},
            {NativeClassID.Shader, typeof(ShaderExporter) },
            {NativeClassID.MonoBehaviour, typeof(BehaviorExporter) },
            {NativeClassID.GameObject, typeof(GltfExporter)},
            {NativeClassID.Mesh, typeof(GltfExporter)},
            {NativeClassID.Animator, typeof(GltfExporter)},
            {NativeClassID.AnimationClip, typeof(GltfExporter)},
            {NativeClassID.MovieTexture, typeof(MovieExporter)},
            {NativeClassID.Texture2D, typeof(TextureExporter)},
            {NativeClassID.Sprite, typeof(TextureExporter)},
        };

        private readonly Dictionary<Type, IMultipartExporter> _batchItems = [];


        internal void ExportAssets(string folder, ArchiveExtractMode mode, CancellationToken token)
        {
            foreach (var asset in _assetItems)
            {
                for (var i = 0; i < asset.Count; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        Logger?.Info("Exporting assets has been cancelled !!");
                        return;
                    }
                    var info = asset.Get(i);
                    if (IsExclude(info.FileID))
                    {
                        continue;
                    }
                    try
                    {
                        var obj = asset[i];
                        if (obj is null)
                        {
                            // 默认 object 不做转化
                            continue;
                        }
                        var fileName = string.IsNullOrEmpty(obj.Name) ? info.FileID.ToString() : obj.Name;
                        var exporter = TryParse(asset, i);
                        exporter?.SaveAs(_fileItems.Create(FileNameHelper.Create(asset.FullPath,
                            string.IsNullOrEmpty(exporter.FileName) ? fileName : exporter.FileName
                        ), folder), mode);
                        if (exporter is IDisposable d)
                        {
                            d?.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        Logger?.Debug(asset.FullPath);
                        Logger?.Error(e.Message);
                    }
                }
                if (token.IsCancellationRequested)
                {
                    return;
                }
                foreach (var batch in _batchItems)
                {
                    batch.Value.SaveAs(_fileItems.Create(FileNameHelper.Create(asset.FullPath, 
                        batch.Value.FileName), folder), mode);
                    batch.Value.Dispose();
                }
                _batchItems.Clear();
            }
        }

        private IMultipartExporter? TryParseModel(ISerializedFile asset)
        {
            return Options?.ModelFormat.ToLower() switch
            {
                "fbx" => new FbxExporter(asset),
                _ => new GltfExporter(asset),
            };
        }


        private IBundleExporter? TryParse(ISerializedFile asset, int objIndex)
        {
            IMultipartExporter? instance;
            var info = asset.Get(objIndex);
            var cls = (NativeClassID)info.ClassID;
            if (cls is NativeClassID.GameObject or NativeClassID.Mesh 
                or NativeClassID.Animator or NativeClassID.AnimationClip)
            {
                instance = TryParseModel(asset);
                if (instance is not null)
                {
                    TryAppend(instance, objIndex);
                    return instance;
                }
            }
            if (!_exportItems.TryGetValue(cls, out var targetType))
            {
                return null;
            }
            if (_batchItems.TryGetValue(targetType, out instance))
            {
                TryAppend(instance, objIndex);
                return null;
            }
            var fn = targetType.GetConstructor([typeof(int), typeof(ISerializedFile)]);
            object? target;
            if (fn is not null)
            {
                target = fn?.Invoke([objIndex, asset]);
            }
            else
            {
                target = targetType.GetConstructor([])?.Invoke([]);
            }
            if (target is not IMultipartExporter m)
            {
                return (IBundleExporter)target;
            }
            if (fn is null)
            {
                TryAppend(target, objIndex);
            }
            _batchItems.TryAdd(targetType, m);
            return null;
        }

        private static void TryAppend(object instance, int objIndex)
        {
            instance.GetType().GetMethod("Append",
                    [objIndex.GetType()])?.Invoke(instance, [objIndex]);
        }
    }
}
