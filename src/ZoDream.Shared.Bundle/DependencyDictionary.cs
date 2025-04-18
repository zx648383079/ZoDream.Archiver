﻿using System.Collections.Generic;

namespace ZoDream.Shared.Bundle
{
    public class DependencyEntry(string fileName, long offset, IList<string> dependencies) : IDependencyEntry
    {
        public string Path { get; private set; } = fileName;

        public long Offset { get; private set; } = offset;

        public IList<string> Dependencies { get; private set; } = dependencies;


        public void Add(params string[] dependencies)
        {
            foreach (var item in dependencies)
            {
                if (Dependencies.Contains(item))
                {
                    continue;
                }
                Dependencies.Add(item);
            }
        }
    }

    public class DependencyDictionary: Dictionary<string, IDependencyEntry>, IDependencyDictionary
    {

        public string FileName { get; set; } = string.Empty;
        public string Entrance { get; set; } = string.Empty;

        public void Add(string key, params string[] dependencies)
        {
            if (TryGetValue(key, out var entry))
            {
                entry.Add(dependencies);
                return;
            }
            Add(key, new DependencyEntry(key, 0, dependencies));
        }

        public bool TryGet(string fileName, out string[] items)
        {
            if (!TryGetValue(fileName, out var entry))
            {
                items = [];
                return false;
            }
            items = [.. entry.Dependencies];
            return true;
        }
    }
}
