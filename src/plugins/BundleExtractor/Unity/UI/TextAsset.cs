using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class TextAsset : NamedObject, IFileWriter
    {
        public Stream Script;

        public TextAsset(UIReader reader) : base(reader)
        {
            Script = reader.ReadAsStream();
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".txt";
            }
            if (!LocationStorage.TryCreate(fileName, extension, mode, out fileName))
            {
                return;
            }
            Script.SaveAs(fileName);
        }
    }
}
