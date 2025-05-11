using Mono.Cecil;
using System;

namespace ZoDream.BundleExtractor
{
    public interface IAssemblyReader : IDisposable
    {

        public void Load(string folder);

        public TypeDefinition? GetType(string assemblyName, string fullName);
    }
}
