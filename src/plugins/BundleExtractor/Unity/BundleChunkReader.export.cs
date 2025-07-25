﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using ZoDream.BundleExtractor.Unity;
using ZoDream.BundleExtractor.Unity.Exporters;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Logging;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

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
            //{NativeClassID.GameObject, typeof(GltfExporter)},
            //{NativeClassID.Mesh, typeof(GltfExporter)},
            //{NativeClassID.Animator, typeof(GltfExporter)},
            //{NativeClassID.AnimationClip, typeof(GltfExporter)},
            {NativeClassID.MovieTexture, typeof(MovieExporter)},
            {NativeClassID.Texture2D, typeof(TextureExporter)},
            {NativeClassID.Sprite, typeof(TextureExporter)},
        };
        private readonly List<IMultipartExporter> _exporterItems = [];
        /// <summary>
        /// 导出资源
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="mode"></param>
        /// <param name="token"></param>
        internal void ExportResource(string folder, ArchiveExtractMode mode, CancellationToken token)
        {
            if (!Options.EnabledResource)
            {
                return;
            }
            var progress = Logger?.CreateSubProgress("Export resources...", _resourceItems.Count);
            foreach (var item in _resourceItems.Values)
            {
                if (item.Value.Position > 0)
                {
                    continue;
                }
                var fileName = _fileItems.Create(item.Key, string.Empty, folder);
                if (!string.IsNullOrEmpty(Path.GetExtension(fileName)) && LocationStorage.TryCreate(fileName, ArchiveExtractMode.Skip, out fileName))
                {
                    item.Value.SaveAs(fileName);
                }
                if (progress is not null)
                {
                    progress.Value++;
                }
            }
        }
        internal void ExportAssets(string folder, ArchiveExtractMode mode, CancellationToken token)
        {
            var progress = Logger?.CreateSubProgress("Batch Export ...", _exporterItems.Count);
            foreach (var exporter in _exporterItems)
            {
                if(!exporter.IsEmpty)
                {
                    exporter.SaveAs(_fileItems.Create(exporter.SourcePath,
                            exporter.FileName
                        , folder), mode);
                }
                exporter.Dispose();
            }
            _exporterItems.Clear();
            progress = Logger?.CreateSubProgress("Export assets...", _assetItems.Count);
            foreach (var asset in _assetItems)
            {
                var sourcePath = FilePath.GetFilePath(asset.FullPath);
                if (!_fileItems.IsExportable(sourcePath))
                {
                    continue;
                }
                for (var i = 0; i < asset.Count; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        Logger?.Info("Exporting assets has been cancelled !!");
                        return;
                    }
                    
                    if (asset.IsExclude(i))
                    {
                        continue;
                    }
                    var info = asset.Get(i);
                    try
                    {
                        var obj = asset[i];
                        if (obj is null)
                        {
                            // 默认 object 不做转化
                            continue;
                        }
                        var fileName = string.IsNullOrEmpty(obj.Name) ? info.FileID.ToString() 
                            : obj.Name;
                        var exporter = TryParse(asset, i);
                        exporter?.SaveAs(_fileItems.Create(asset.FullPath,
                            string.IsNullOrEmpty(exporter.FileName) ? fileName : 
                            exporter.FileName
                        , folder), mode);
                        if (exporter is IDisposable d)
                        {
                            d?.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        Logger?.Log(LogLevel.Error, e, $"<{info.TypeID}>{info.FileID} of {asset.FullPath}");
                    }
                }
                if (token.IsCancellationRequested)
                {
                    return;
                }
                _fileItems.Filter?.Exclude(sourcePath, BundleExcludeFlag.Export);
                if (progress is not null)
                {
                    progress.Value++;
                }
                Shared.Clear();
            }
        }
        /// <summary>
        /// 一些涉及多个对象的批量导出
        /// </summary>
        /// <param name="entryId"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        private bool PrepareExport(int entryId, ISerializedFile resource)
        {
            if (resource.IsExclude(entryId) || 
                !_fileItems.IsExportable(FilePath.GetFilePath(resource.FullPath)))
            {
                return false;
            }
            if (resource[entryId] is not MonoBehaviour behaviour 
                || !behaviour.Script.TryGet(out var script) 
                || behaviour.GameObject?.Index < 0)
            {
                return false;
            }
            if (script.ClassName.StartsWith("Cubism"))
            {
                _exporterItems.Add(new CubismExporter(behaviour.GameObject));
                return true;
            }
            if (script.NameSpace.StartsWith("Spine."))
            {
                _exporterItems.Add(new SpineExporter(behaviour.GameObject));
                return true;
            }
            if (script.NameSpace.StartsWith("Utage"))
            {
                _exporterItems.Add(new UtageExporter(entryId, resource));
                return true;
            }
            return false;
        }

        private IMultipartBuilder? TryParseModel(ISerializedFile asset)
        {
            if (Options?.EnabledModel != true)
            {
                return null;
            }
            return Options?.ModelFormat?.ToLower() switch
            {
                "fbx" => new FbxExporter(asset),
                _ => new GltfExporter(asset),
            };
        }

        private IBundleExporter? TryParse(ISerializedFile asset, int objIndex)
        {
            //IMultipartExporter? instance;
            var info = asset.Get(objIndex);
            var cls = (NativeClassID)info.ClassID;
            if (cls is NativeClassID.GameObject
                or NativeClassID.Animator or NativeClassID.AnimationClip)
            {
                var instance = TryParseModel(asset);
                if (instance is not null)
                {
                    instance.Append(objIndex);
                    return instance;
                }
            }
            if (cls is NativeClassID.Mesh && Options?.EnabledMesh == true)
            {
                return new MeshExporter(objIndex, asset);
            }
            if (!_exportItems.TryGetValue(cls, out var targetType))
            {
                return null;
            }
            var fn = targetType.GetConstructor([typeof(int), typeof(ISerializedFile)]);
            if (fn is not null)
            {
                return (IBundleExporter)fn?.Invoke([objIndex, asset]);
            }
            return null;
        }

    }
}
