using System.IO;
using ZoDream.Shared.Storage;

namespace Il2CppDumper
{
    public static class DummyAssemblyExporter
    {
        public static void Export(Il2CppExecutor il2CppExecutor, 
            string outputDir, bool addToken)
        {
            var folder = Path.Combine(outputDir, "DummyDll");
            LocationStorage.CreateDirectory(folder);
            var dummy = new DummyAssemblyGenerator(il2CppExecutor, addToken);
            foreach (var assembly in dummy.Assemblies)
            {
                using var fs = File.Create(Path.Combine(folder, assembly.MainModule.Name));
                assembly.Write(fs);
            }
        }
    }
}
