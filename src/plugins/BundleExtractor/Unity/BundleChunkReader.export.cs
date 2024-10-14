using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor
{
    public partial class UnityBundleChunkReader
    {
        public bool ExportRawFile(UIObject item, string exportPath, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(exportPath,  ".dat", mode, out var exportFullPath))
            {
                return false;
            }
            item.GetRawData().SaveAs(exportFullPath);
            return true;
        }

        public bool ExportConvertFile(UIObject item, string exportPath, ArchiveExtractMode mode)
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
                case ElementIDType.MonoBehaviour:
                    ((MonoBehaviour)item)?.SaveAs(exportPath, mode);
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

        public bool ExportJSONFile(UIObject item, string exportPath, ArchiveExtractMode mode)
        {
            //if (!TryExportFile(exportPath, item, ".json", out var exportFullPath))
            //    return false;

            //var settings = new JsonSerializerSettings();
            //settings.Converters.Add(new StringEnumConverter());
            //var str = JsonConvert.SerializeObject(item, Formatting.Indented, settings);
            //File.WriteAllText(exportFullPath, str);
            return true;
        }

    }
}
