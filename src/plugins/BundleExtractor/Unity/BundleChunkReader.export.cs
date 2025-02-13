using System;
using System.Collections.Generic;
using System.Threading;
using ZoDream.BundleExtractor.Unity;
using ZoDream.BundleExtractor.Unity.Exporters;
using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor
{
    internal partial class UnityBundleChunkReader
    {

        private readonly Dictionary<Type, Type> _exportItems = new() 
        {
            {typeof(TextAsset), typeof(LuaExporter)},
            {typeof(AudioClip), typeof(FsbExporter)},
            {typeof(Shader), typeof(ShaderExporter) },
            {typeof(GameObject), typeof(GltfExporter)},
            {typeof(Mesh), typeof(GltfExporter)},
            {typeof(Animator), typeof(GltfExporter)},
            {typeof(AnimationClip), typeof(GltfExporter)},
        };

        internal void ExportAssets(string folder, ArchiveExtractMode mode, CancellationToken token)
        {
            var batchItems = new Dictionary<Type, IMultipartExporter>();
            foreach (var asset in _assetItems)
            {
                foreach (var obj in asset.Children)
                {
                    if (token.IsCancellationRequested)
                    {
                        Logger.Info("Exporting assets has been cancelled !!");
                        return;
                    }
                    try
                    {
                        var exportPath = _fileItems.Create(FileNameHelper.Create(asset.FullPath, obj.Name), folder);
                        if (_exportItems.TryGetValue(obj.GetType(), out var targetType))
                        {
                            if (batchItems.TryGetValue(targetType, out var instance))
                            {
                                targetType.GetMethod("Append",
                                    [obj.GetType()])?.Invoke(instance, [obj]);
                            }
                            var target = targetType.GetConstructor([obj.GetType()])?.Invoke([obj]);
                            target ??= targetType.GetConstructor([])?.Invoke([]);
                            if (target is IMultipartExporter m)
                            {
                                targetType.GetMethod("Append",
                                    [obj.GetType()])?.Invoke(m, [obj]);
                                batchItems.Add(targetType, m);
                                continue;
                            }
                            else if (target is IFileExporter f)
                            {
                                f.SaveAs(exportPath, mode);
                                continue;
                            }
                        }
                        ExportConvertFile(obj, exportPath, mode);
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
                foreach (var batch in batchItems)
                {
                    batch.Value.SaveAs(_fileItems.Create(FileNameHelper.Create(asset.FullPath, batch.Value.FileName), folder), mode);
                }
                batchItems.Clear();
            }
        }

        internal bool ExportRawFile(UIObject item, string exportPath, ArchiveExtractMode mode)
        {
            return false;
            if (!LocationStorage.TryCreate(exportPath,  ".dat", mode, out var exportFullPath))
            {
                return false;
            }
            var stream = item.GetRawData();
            if (stream.Length == 0)
            {
                return false;
            }
            stream.SaveAs(exportFullPath);
            return true;
        }

        internal bool ExportConvertFile(UIObject item, string exportPath, ArchiveExtractMode mode)
        {
            switch (item.Type)
            {
                case ElementIDType.GameObject:
                    ((GameObject)item)?.SaveAs(exportPath, mode);
                    return true;
                case ElementIDType.Texture2D:
                    ((Texture2D)item)?.SaveAs(exportPath, mode);
                    return true;
                case ElementIDType.AudioClip:
                    ((AudioClip)item)?.SaveAs(exportPath, mode);
                    return true;
                case ElementIDType.Shader:
                    ((Shader)item)?.SaveAs(exportPath, mode);
                    return true;
                case ElementIDType.TextAsset:
                    ((TextAsset)item)?.SaveAs(exportPath, mode);
                    return true;
                case ElementIDType.MonoBehavior:
                    ((MonoBehavior)item)?.SaveAs(exportPath, mode);
                    return true;
                case ElementIDType.Font:
                    ((Font)item)?.SaveAs(exportPath, mode);
                    return true;
                case ElementIDType.Mesh:
                    ((Mesh)item)?.SaveAs(exportPath, mode);
                    return true;
                case ElementIDType.VideoClip:
                    ((VideoClip)item)?.SaveAs(exportPath, mode);
                    return true;
                case ElementIDType.MovieTexture:
                    ((MovieTexture)item)?.SaveAs(exportPath, mode);
                    return true;
                case ElementIDType.Sprite:
                    ((Sprite)item)?.SaveAs(exportPath, mode);
                    return true;
                case ElementIDType.Animator:
                    ((Animator)item)?.SaveAs(exportPath, mode);
                    return true;
                case ElementIDType.AnimationClip:
                    ((AnimationClip)item)?.SaveAs(exportPath, mode);
                    return true;
                case ElementIDType.MiHoYoBinData:
                    ((MiHoYoBinData)item)?.SaveAs(exportPath, mode);
                    return true;
                case ElementIDType.Material:
                    return ExportJSONFile(item, exportPath, mode);
                default:
                    return ExportRawFile(item, exportPath, mode);
            }
        }

        internal bool ExportJSONFile(UIObject item, string exportPath, ArchiveExtractMode mode)
        {
            //if (!TryExportFile(exportPath, item, ".json", out var exportFullPath))
            //    return false;

            //var settings = new JsonSerializerSettings();
            //settings.Converters.Add(new StringEnumConverter());
            //var str = JsonConvert.SerializeObject(item, Formatting.Indented, settings);
            //File.WriteAllText(exportFullPath, str);
            // TODO
            return true;
        }

    }
}
