using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZoDream.Shared.Compression.Zip;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.EpubExtractor
{
    public partial class EpubWriter : IArchiveWriter
    {
        public EpubWriter(Stream stream, IArchiveOptions options)
        {
            _writer = new(stream, options);
            WriteHeader();
        }

        private readonly ZipArchiveWriter _writer;
        private readonly Encoding _encoding = new UTF8Encoding(false);
        private readonly string _uuid = UUID();
        private readonly List<ChapterItem> _chapterItems = [];
        private readonly Dictionary<string, string> _fileItems = [];
        private int _sectionIndex = 0;
        private string _sectionName = "第一卷";

        public IReadOnlyEntry AddEntry(string name, string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                WriteSection();
                // 分卷名
                _sectionName = name;
                _sectionIndex++;
                return new ReadOnlyEntry(name);
            }
            using var fs = File.OpenRead(fullPath);
            return _writer.AddEntry(name, fs);
        }

        public IReadOnlyEntry AddEntry(string name, Stream input)
        {
            name = name.Replace('\\', '/');
            var id = name.Split('/').Last();
            if (_fileItems.ContainsKey(id))
            {
                id = name.Replace('/', '_');
            }
            if (name.EndsWith("html"))
            {
                _chapterItems.Add(new(ReadTitle(input), id));
            }
            _fileItems.Add(id, name);
            return _writer.AddEntry("OEBPS/" + name, input);
        }


        private void WriteHeader()
        {
            AddText("mimetype", "application/epub+zip");
        }

        private string ReadTitle(Stream input)
        {
            var text = new StreamReader(input).ReadToEnd();
            var match = TitleRegex().Match(text);
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private IReadOnlyEntry AddText(string name, string content)
        {
            using var ms = new MemoryStream();
            ms.Write(_encoding.GetBytes(content));
            return _writer.AddEntry(name, ms);
        }

        private void WriteSection()
        {
            if (_fileItems.Count == 0)
            {
                return;
            }
            AddText($"content{RenderSection(_sectionIndex)}.opf", RenderOPF());
            AddText($"toc{RenderSection(_sectionIndex)}.ncx", RenderNCX());
            _chapterItems.Clear();
            _fileItems.Clear();
        }

        private string RenderSection(int index)
        {
            if (index > 0)
            {
                return index.ToString();
            }
            return string.Empty;
        }

        public void Dispose()
        {
            WriteSection();
            AddText("META-INF/container.xml", RenderContainer());
            _writer.Dispose();
        }

        [GeneratedRegex(@"\<title\>(.+?)\</title\>")]
        private static partial Regex TitleRegex();
    }
}
