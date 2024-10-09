using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.UI
{
    public sealed class TextAsset : NamedObject, IFileWriter
    {
        public Stream Script;

        public TextAsset(UIReader reader) : base(reader)
        {
            Script = reader.ReadAsStrem();
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, ".txt", mode, out fileName))
            {
                return;
            }
            Script.SaveAs(fileName);
        }
    }
}
