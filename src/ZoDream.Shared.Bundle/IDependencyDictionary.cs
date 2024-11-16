﻿using System.Collections.Generic;

namespace ZoDream.Shared.Bundle
{
    public interface IDependencyEntry
    {
        public string Path { get; }

        public IList<string> Dependencies { get; }

        public void Add(params string[] dependencies);
    }

    public interface IDependencyDictionary : IDictionary<string, IDependencyEntry>
    {

        public string Entrance { get; }

        public void Add(string key, params string[] dependencies);
    }
}
