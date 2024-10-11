using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using ZoDream.BundleExtractor.Platforms;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor
{
    public partial class UnityBundleReader : IBundleReader
    {
 

        public UnityBundleReader(
            IBundlePlatform platform, 
            IArchiveOptions? options)
        {
            _platform = platform;
            _options = options;
        }

        public UnityBundleReader(UnityBundleScheme scheme, IBundlePlatform platform,
            IArchiveOptions? options): this(platform, options)
        {
            _scheme = scheme;
        }

        private readonly IBundlePlatform _platform;
        private readonly IArchiveOptions? _options;
        private readonly UnityBundleScheme? _scheme;

        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
        {
            foreach (var items in _platform.Producer.EnumerateChunk())
            {
                using var chunk = new BundleChunkReader(items, _scheme, _platform);
                chunk.ExtractTo(folder, mode, token);
            }
        }

        public void Dispose()
        {
        }

        private static Stream OpenRead(string fileName)
        {
            var name = Path.GetFileName(fileName);
            if (!SplitFileRegex().IsMatch(name))
            {
                return File.OpenRead(fileName);
            }
            var folder = Path.GetDirectoryName(fileName);
            var i = name.LastIndexOf('t');
            var items =
                Directory.GetFiles(folder, name[..i] + '*')
                .OrderBy(j => int.Parse(Path.GetFileName(j)[(i + 1)..])).ToArray();
            return MultipartFileStream.Open(items);
        }


        [GeneratedRegex(@"\.split\d+$")]
        private static partial Regex SplitFileRegex();
    }
}
