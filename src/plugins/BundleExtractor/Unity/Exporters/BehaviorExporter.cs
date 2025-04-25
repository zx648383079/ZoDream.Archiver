using System.IO;
using System.Text.Json;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.Exporters.Cecil;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class BehaviorExporter : IBundleExporter
    {
        public BehaviorExporter(int entryId, ISerializedFile resource)
        {
            _behavior = resource[entryId] as MonoBehaviour;
        }

        private readonly MonoBehaviour _behavior;

        public string FileName => _behavior.Name;

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (_behavior.Script is null || !_behavior.Script.TryGet(out var script))
            {
                return;
            }
            if (!LocationStorage.TryCreate(fileName, ".json", mode, out fileName))
            {
                return;
            }
            switch (script.ClassName)
            {
                case "CubismMoc":
                    new CubismExporter(_behavior).SaveAs(fileName, mode);
                    return;
                case "CubismPhysicsController":
                    break;
                case "CubismExpressionData":
                    break;
                case "CubismFadeMotionData":
                    break;
                case "CubismFadeMotionList":
                    break;
                case "CubismEyeBlinkParameter":
                    break;
                case "CubismMouthParameter":
                    break;
                case "CubismParameter":
                    break;
                case "CubismPart":
                    break;
                case "CubismDisplayInfoParameterName":
                    break;
                case "CubismDisplayInfoPartName":
                    break;
            }
            var type = _behavior.ToType();
            if (type == null)
            {
                var loader = _behavior.Reader.Get<AssemblyLoader>();
                if (loader is null)
                {
                    return;
                }
                var m_Type = ConvertToTypeTree(_behavior, loader);
                type = _behavior.ToType(m_Type);
            }
            
            using var fs = File.Create(fileName);
            JsonSerializer.Serialize(fs, type);
        }



        public static TypeTree ConvertToTypeTree(MonoBehaviour m_MonoBehavior, 
            AssemblyLoader assemblyLoader)
        {
            var m_Type = new TypeTree();
            var helper = new SerializedTypeHelper(m_MonoBehavior.Version);
            helper.AddMonoBehavior(m_Type.Nodes, 0);
            if (m_MonoBehavior.Script.TryGet(out var m_Script))
            {
                var typeDef = assemblyLoader.GetTypeDefinition(m_Script.AssemblyName, string.IsNullOrEmpty(m_Script.m_Namespace) ? m_Script.m_ClassName : $"{m_Script.m_Namespace}.{m_Script.m_ClassName}");
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
