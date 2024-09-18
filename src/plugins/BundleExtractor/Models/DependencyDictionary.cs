using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BundleExtractor.Models
{
    public class DependencyEntry(string fileName, long offset, IList<string> dependencies)
    {
        public string Path { get; private set; } = fileName;

        public long Offset { get; private set; } = offset;

        public IList<string> Dependencies { get; private set; } = dependencies;
    }

    internal class UTY_DependencyFile
    {
        public string[] files;

        public UTY_DependencyMap[] assetbundles;
    }
    internal class UTY_DependencyMap
    {
        public int n;
        public int[] d;
    }

    public class DependencyDictionary: Dictionary<string, DependencyEntry>
    {

        public string FileName { get; set; } = string.Empty;
        public string BaseFolder { get; private set; } = string.Empty;


        public void Load(string fileName)
        {
            FileName = Path.GetFullPath(fileName);
            if (!fileName.EndsWith(".json"))
            {
                using var fs = File.OpenRead(fileName);
                Load(fs);
                return;
            }
            BaseFolder = Path.GetDirectoryName(fileName)!;
            var items = JsonSerializer.Deserialize<UTY_DependencyFile>(File.ReadAllText(fileName));
            if (items is null)
            {
                return;
            }
            foreach (var item in items.assetbundles)
            {
                var cab = items.files[item.n];
                if (string.IsNullOrEmpty(fileName))
                {
                    continue;
                }
                Add(cab, new(cab, 0, item.d.Select(i => items.files[i]).ToArray()));
            }
        }

        public void Load(Stream input)
        {
            using var reader = new BinaryReader(input);
            BaseFolder = reader.ReadString();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var cab = reader.ReadString();
                var path = reader.ReadString();
                var offset = reader.ReadInt64();
                var depCount = reader.ReadInt32();
                var dependencies = new string[depCount];
                for (int j = 0; j < depCount; j++)
                {
                    dependencies[i] = reader.ReadString();
                }
                TryAdd(cab, new(path, offset, dependencies));
            }
        }
    }
}
