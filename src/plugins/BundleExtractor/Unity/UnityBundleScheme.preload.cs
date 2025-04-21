using System.IO;
using ZoDream.Shared.IO;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using System.Text.Json;

namespace ZoDream.BundleExtractor
{
    public partial class UnityBundleScheme
    {

        /*
         * 预处理流程
         * 1. 解压 bundle
         * 2. 合并 split 文件
         * 3. 判断 SerializedFile 建立文件依赖表
         * 4. 根据依赖表拆分文件组进行处理
         */
        public static void Preload(
            IBundleSource source, 
            IBundleOptions options)
        {
            var dependencyFile = Path.Combine(options.Entrance!, "dependency.temp");
            if (File.Exists(dependencyFile))
            {
                return;
            }
            var scheme = new SerializedFileScheme();
            var dependencies = new DependencyDictionary();
            foreach (var item in source.GetFiles())
            {
                if (TryGetSplitTemporaryFile(item, out var fileName))
                {
                    if (!File.Exists(fileName))
                    {
                        using var output = File.Create(fileName);
                        using var input = OpenSplitStream(fileName);
                        input.CopyTo(output);
                    }
                } else
                {
                    fileName = item;
                }
                using var fs = File.OpenRead(fileName);
                var targetFolder = fileName + "_temp";
                if (!Directory.Exists(targetFolder))
                {
                    using var r = OpenBundle(fs);
                    if (r != null)
                    {
                        Directory.CreateDirectory(targetFolder);
                        r.ExtractToDirectory(targetFolder);
                    }
                    continue;
                }
                fs.Seek(0, SeekOrigin.Begin);
                using var reader = scheme.Open(fs, fileName, Path.Combine(fileName));
                if (reader is SerializedFileReader s)
                {
                    foreach (var d in s.Dependencies)
                    {
                        dependencies.Add(fileName, d);
                    }
                }
            }
            using var saver = File.Create(dependencyFile);
            JsonSerializer.Serialize(saver, dependencies);
        }

        public static bool TryGetSplitTemporaryFile(string fileName, out string temporaryFileName)
        {
            var match = SplitFileRegex().Match(fileName);
            if (!match.Success)
            {
                temporaryFileName = string.Empty;
                return false;
            }
            temporaryFileName = fileName[..^match.Value.Length] + "._s_temp";
            return true;
        }
    }
}
