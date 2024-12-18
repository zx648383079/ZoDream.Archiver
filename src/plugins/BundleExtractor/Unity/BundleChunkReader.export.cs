﻿using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor
{
    internal partial class UnityBundleChunkReader
    {
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
