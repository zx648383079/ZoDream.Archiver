using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.AutodeskExporter
{
    public class FbxReader(string fileName) : IArchiveReader
    {
        public bool Read()
        {
            using var manager = new FbxManager();
            using var importer = new FbxImporter(manager, "");
            var setting = new FbxIOSettings(manager, FbxIOSettings.IOSROOT);
            manager.IOSettings = setting;
            var scene = new FbxScene(manager, "my_scene");
            if (!importer.Initialize(fileName, -1, setting))
            {
                return false;
            }
            importer.GetFileVersion(out var major, out var minor, out var revision);
            if (!importer.IsFBX || !importer.Import(scene))
            {
                return false;
            }
            return true;
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            throw new NotImplementedException();
        }

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            throw new NotImplementedException();
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
