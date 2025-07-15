using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace ZoDream.BundleExtractor
{
    public interface IAssemblyReader : IDisposable
    {

        public void Load(string folder);

        public TypeDefinition? GetType(string assemblyName, string fullName);
        public IEnumerable<TypeDefinition> GetType(string assemblyName);
    }
}
