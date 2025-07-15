using Mono.Cecil;
using System.Collections.Generic;
using System.IO;

namespace ZoDream.BundleExtractor.Unity.Document
{
    /// <summary>
    /// https://github.com/Perfare/Il2CppDumper 先提取 dll
    /// </summary>
    public class AssemblyReader : IAssemblyReader
    {
        public bool IsLoaded { get; private set; }
        private readonly Dictionary<string, ModuleDefinition> _moduleItems = [];

        public void Load(string folder)
        {
            var files = Directory.GetFiles(folder, "*.dll");
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

        public IEnumerable<TypeDefinition> GetType(string assemblyName)
        {
            if (!_moduleItems.TryGetValue(assemblyName, out var module))
            {
                yield break;
            }
            foreach (var item in module.Types)
            {
                yield return item;
            }
        }

        public TypeDefinition? GetType(string assemblyName, string fullName)
        {
            if (!_moduleItems.TryGetValue(assemblyName, out var module))
            {
                return null;
            }
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

        public void Dispose()
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
