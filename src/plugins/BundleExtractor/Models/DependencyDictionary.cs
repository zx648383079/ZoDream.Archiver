using System.Collections.Generic;
using ZoDream.Shared.Bundle;

namespace BundleExtractor.Models
{
    public class DependencyEntry(string fileName, long offset, IList<string> dependencies) : IDependencyEntry
    {
        public string Path { get; private set; } = fileName;

        public long Offset { get; private set; } = offset;

        public IList<string> Dependencies { get; private set; } = dependencies;
    }

    

    public class DependencyDictionary: Dictionary<string, IDependencyEntry>, IDependencyDictionary
    {

        public string FileName { get; set; } = string.Empty;
        public string Entrance { get; set; } = string.Empty;
    }
}
