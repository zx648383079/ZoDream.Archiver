using System.IO;
using System.Text.Json;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.BundleExtractor.Unity.Exporters.Cecil;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
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
                var m_Type = ConvertToTypeTree(behavior, loader, resource);
                type = UnityConverter.ToType(m_Type, entryId, resource);
            }
            using var fs = File.Create(fileName);
            JsonSerializer.Serialize(fs, type, JsonExporter.Options);
        }



        public static TypeTree ConvertToTypeTree(MonoBehaviour m_MonoBehavior, 
            AssemblyLoader assemblyLoader, ISerializedFile resource)
        {
            var m_Type = new TypeTree();
            var helper = new SerializedTypeHelper(resource.Version);
            helper.AddMonoBehavior(m_Type.Nodes, 0);
            if (m_MonoBehavior.Script.TryGet(out var m_Script))
            {
                var typeDef = assemblyLoader.GetTypeDefinition(m_Script.AssemblyName, 
                    string.IsNullOrEmpty(m_Script.NameSpace) ? m_Script.ClassName : $"{m_Script.NameSpace}.{m_Script.ClassName}");
                if (typeDef != null)
                {
                    var typeDefinitionConverter = new TypeDefinitionConverter(typeDef, helper, 1);
                    m_Type.Nodes.AddRange(typeDefinitionConverter.ConvertToTypeTreeNodes());
                }
            }
            return m_Type;
        }

    }
}
