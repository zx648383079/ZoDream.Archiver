using System;
using System.IO;
using System.Text.Json;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Bundle.Storage
{
    public class BundleManifest
    {
        public string Name { get; set; } = string.Empty;
        public Version Version { get; set; } = new Version(1, 0);

        public BundleOptions Options { get; set; } = new();

        public string[] Files { get; set; } = [];

        public uint Skip {  get; set; } = 0;
    }

    public class BundleManifestReader : IManifestReader<BundleManifest>, IManifestWriter<BundleManifest>
    {
        public BundleManifest? ReadFrom(Stream input)
        {
            return JsonSerializer.Deserialize<BundleManifest>(input);
        }

        public BundleManifest? ReadFrom(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return null;
            }
            using var fs = File.OpenRead(fileName);
            return ReadFrom(fs);
        }

        public void WriteTo(Stream output, BundleManifest data)
        {
            JsonSerializer.Serialize(output, data);
        }

        public void WriteTo(string outputPath, BundleManifest data)
        {
            using var fs = File.Create(outputPath);
            WriteTo(fs, data);
        }
    }
}
