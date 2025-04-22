using System.IO;
using System.Text.Json;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.Exporters.Cecil;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class BehaviorExporter(MonoBehavior behavior) : IBundleExporter
    {
        public string Name => behavior.Name;

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (behavior.m_Script is null || !behavior.m_Script.TryGet(out var script))
            {
                return;
            }
            if (!LocationStorage.TryCreate(fileName, ".json", mode, out fileName))
            {
                return;
            }
            switch (script.m_ClassName)
            {
                case "CubismMoc":
                    new CubismExporter(behavior).SaveAs(fileName, mode);
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
            var type = behavior.ToType();
            if (type == null)
            {
                var loader = behavior.Reader.Get<AssemblyLoader>();
                if (loader is null)
                {
                    return;
                }
                var m_Type = ConvertToTypeTree(behavior, loader);
                type = behavior.ToType(m_Type);
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
