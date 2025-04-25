using System.IO;
using UnityEngine;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class FontExporter(int entryId, ISerializedFile resource) : IBundleExporter
    {
        private readonly Font _model = resource[entryId] as Font;
        public string FileName => _model.Name;

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (_model.Data is null)
            {
                return;
            }
            var extension = ".ttf";
            var buffer = _model.Data.ReadBytes(4);
            if (buffer.Equal([0x4F, 0x54, 0x54, 0x4F]))
            {
                extension = ".otf";
            }
            else if (buffer.Equal([0x74,0x74,0x63,0x66]))
            {
                extension = ".ttc";
            }
            else if (buffer.Equal([0x77, 0x4F, 0x46, 0x46]))
            {
                extension = ".woff";
            }
            else if (buffer.Equal([0x74, 0x4F, 0x46, 0x32]))
            {
                extension = ".woff2";
            }
            if (!LocationStorage.TryCreate(fileName, extension, mode, out fileName))
            {
                return;
            }
            using var fs = File.Create(fileName);
            fs.Write(buffer);
            _model.Data.CopyTo(fs);
        }
    }
}
