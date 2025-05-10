using System.IO;
using System.Text.Json;
using UnityEngine;
using UnityEngine.Document;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.BundleExtractor.Unity.Document;
using ZoDream.BundleExtractor.Unity.Document.Cecil;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class BehaviorExporter(int entryId, ISerializedFile resource) : IBundleExporter
    {
        public string FileName => resource[entryId].Name;

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (resource[entryId] is not MonoBehaviour behavior)
            {
                return;
            }
            if (behavior.Script is null 
                || !behavior.Script.TryGet(out var script) 
                || script.ClassName.StartsWith("Cubism"))
            {
                return;
            }
            if (!LocationStorage.TryCreate(fileName, ".json", mode, out fileName))
            {
                return;
            }
            
            var type = UnityConverter.ToType(entryId, resource);
            if (type == null)
            {
                var loader = resource.Container?.Service?.Get<AssemblyLoader>();
                if (loader is null)
                {
                    return;
                }
                var doc = ConvertToTypeTree(behavior, loader, resource);
                type = UnityConverter.ToType(doc, entryId, resource);
            }
            using var fs = File.Create(fileName);
            JsonSerializer.Serialize(fs, type, JsonExporter.Options);
        }



        public static VirtualDocument ConvertToTypeTree(MonoBehaviour behavior, 
            AssemblyLoader assemblyLoader, ISerializedFile resource)
        {
            var builder = new DocumentBuilder(resource.Version);
            builder.AddMonoBehavior(0);
            if (behavior.Script.TryGet(out var script))
            {
                var typeDef = assemblyLoader.GetTypeDefinition(script.AssemblyName, 
                    string.IsNullOrEmpty(script.NameSpace) ? script.ClassName : $"{script.NameSpace}.{script.ClassName}");
                if (typeDef != null)
                {
                    var typeDefinitionConverter = new TypeDefinitionConverter(typeDef, builder, 1);
                    typeDefinitionConverter.ConvertTo();
                } else
                {
                    CubismExporter.ConvertToTypeTree(builder, script);
                }
            }
            return builder.ToDocument();
        }



    }
}
