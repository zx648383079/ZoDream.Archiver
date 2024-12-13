using Mono.Cecil;
using System.Collections.Generic;
using System.IO;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class AssemblyLoader
    {
        public bool IsLoaded { get; private set; }
        private Dictionary<string, ModuleDefinition> _moduleItems = [];

        public void Load(string path)
        {
            var files = Directory.GetFiles(path, "*.dll");
            var resolver = new UnityAssemblyResolver();
            var readerParameters = new ReaderParameters
            {
                AssemblyResolver = resolver
            };
            foreach (var file in files)
            {
                try
                {
                    var assembly = AssemblyDefinition.ReadAssembly(file, readerParameters);
                    resolver.Register(assembly);
                    _moduleItems.Add(assembly.MainModule.Name, assembly.MainModule);
                }
                catch
                {
                    // ignored
                }
            }
            IsLoaded = true;
        }

        public TypeDefinition GetTypeDefinition(string assemblyName, string fullName)
        {
            if (_moduleItems.TryGetValue(assemblyName, out var module))
            {
                var typeDef = module.GetType(fullName);
                if (typeDef == null && assemblyName == "UnityEngine.dll")
                {
                    foreach (var pair in _moduleItems)
                    {
                        typeDef = pair.Value.GetType(fullName);
                        if (typeDef != null)
                        {
                            break;
                        }
                    }
                }
                return typeDef;
            }
            return null;
        }

        public void Clear()
        {
            foreach (var pair in _moduleItems)
            {
                pair.Value.Dispose();
            }
            _moduleItems.Clear();
            IsLoaded = false;
        }
    }

    internal class UnityAssemblyResolver : DefaultAssemblyResolver
    {
        public void Register(AssemblyDefinition assembly)
        {
            RegisterAssembly(assembly);
        }
    }
}
