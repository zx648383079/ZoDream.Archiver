using System;
using System.Collections.Generic;
using System.Threading;
using ZoDream.BundleExtractor.Unity;
using ZoDream.BundleExtractor.Unity.Exporters;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor
{
    internal partial class UnityBundleChunkReader
    {

        private readonly Dictionary<Type, Type> _exportItems = new() 
        {
            {typeof(TextAsset), typeof(RawExporter)},
            {typeof(AudioClip), typeof(FsbExporter)},
            {typeof(Shader), typeof(ShaderExporter) },
            {typeof(MonoBehavior), typeof(BehaviorExporter) },
            {typeof(GameObject), typeof(GltfExporter)},
            {typeof(Mesh), typeof(GltfExporter)},
            {typeof(Animator), typeof(GltfExporter)},
            {typeof(AnimationClip), typeof(GltfExporter)},
        };

        private readonly Dictionary<Type, IMultipartExporter> _batchItems = [];


        internal void ExportAssets(string folder, ArchiveExtractMode mode, CancellationToken token)
        {
            foreach (var asset in _assetItems)
            {
                foreach (var obj in asset.Children)
                {
                    if (token.IsCancellationRequested)
                    {
                        Logger.Info("Exporting assets has been cancelled !!");
                        return;
                    }
                    if (IsExclude(obj.FileID))
                    {
                        continue;
                    }
                    try
                    {
                        var fileName = string.IsNullOrEmpty(obj.Name) ? obj.FileID.ToString() : obj.Name;
                        var exporter = TryParse(obj);
                        exporter?.SaveAs(_fileItems.Create(FileNameHelper.Create(asset.FullPath,
                            string.IsNullOrEmpty(exporter.Name) ? fileName : exporter.Name
                        ), folder), mode);
                        if (exporter is IDisposable d)
                        {
                            d?.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Debug(asset.FullPath);
                        Logger.Error(e.Message);
                    }
                }
                if (token.IsCancellationRequested)
                {
                    return;
                }
                foreach (var batch in _batchItems)
                {
                    batch.Value.SaveAs(_fileItems.Create(FileNameHelper.Create(asset.FullPath, 
                        batch.Value.Name), folder), mode);
                    batch.Value.Dispose();
                }
                _batchItems.Clear();
            }
        }

        private IMultipartExporter? TryParseModel()
        {
            return Options?.ModelFormat.ToLower() switch
            {
                "fbx" => new FbxExporter(this),
                _ => new GltfExporter(this),
            };
        }

        private bool IsExclude(UIObject obj)
        {
            return obj switch
            {
                GameObject or Mesh or AnimationClip or Animator => Options?.EnabledModel != true,
                Shader => Options?.EnabledShader != true,
                Texture2D or Texture or Sprite => Options?.EnabledImage != true,
                AudioClip => Options?.EnabledAudio != true,
                VideoClip => Options?.EnabledVideo != true,
                TextAsset => Options?.EnabledLua != true && Options?.EnabledSpine != true && Options?.EnabledJson != true,
                _ => false,
            };
        }

        private IFileExporter? TryParse(UIObject obj)
        {
            if (IsExclude(obj))
            {
                return null;
            }
            IMultipartExporter? instance;
            if (obj is GameObject or Mesh or Animator or AnimationClip)
            {
                instance = TryParseModel();
                if (instance is not null)
                {
                    TryAppend(instance, obj);
                    return instance;
                }
            }
            if (!_exportItems.TryGetValue(obj.GetType(), out var targetType))
            {
                return obj is IFileExporter f ? f : null;
            }
            if (_batchItems.TryGetValue(targetType, out instance))
            {
                TryAppend(instance, obj);
                return null;
            }
            var fn = targetType.GetConstructor([obj.GetType()]);
            object? target;
            if (fn is not null)
            {
                target = fn?.Invoke([obj]);
            }
            else
            {
                target = targetType.GetConstructor([])?.Invoke([]);
            }
            if (target is not IMultipartExporter m)
            {
                return (IFileExporter)target;
            }
            if (fn is null)
            {
                TryAppend(target, obj);
            }
            _batchItems.TryAdd(targetType, m);
            return null;
        }

        private static void TryAppend(object instance, UIObject obj)
        {
            instance.GetType().GetMethod("Append",
                    [obj.GetType()])?.Invoke(instance, [obj]);
        }
    }
}
