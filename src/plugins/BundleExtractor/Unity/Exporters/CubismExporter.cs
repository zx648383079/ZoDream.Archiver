using System.Collections.Specialized;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class CubismExporter(IBundleBinaryReader reader) : IFileExporter
    {
        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, ".moc3", mode, out var exportFullPath))
            {
                return;
            }
            var length = reader.ReadUInt32();
            reader.ReadAsStream(length).SaveAs(exportFullPath);
        }

        public static OrderedDictionary ParseMonoBehavior(MonoBehavior m_MonoBehaviour, CubismMonoBehaviorType cubismMonoBehaviorType, AssemblyLoader assemblyLoader)
        {
            var orderedDict = m_MonoBehaviour.ToType();
            if (orderedDict != null)
            {
                return orderedDict;
            }

            var fieldName = "";
            var m_Type = ConvertToTypeTree(m_MonoBehaviour, assemblyLoader);
            switch (cubismMonoBehaviorType)
            {
                case CubismMonoBehaviorType.FadeMotionList:
                    fieldName = "cubismfademotionobjects";
                    break;
                case CubismMonoBehaviorType.FadeMotion:
                    fieldName = "parameterids";
                    break;
                case CubismMonoBehaviorType.Expression:
                    fieldName = "parameters";
                    break;
                case CubismMonoBehaviorType.Physics:
                    fieldName = "_rig";
                    break;
                case CubismMonoBehaviorType.DisplayInfo:
                    fieldName = "name";
                    break;
            }
            if (m_Type.Nodes.FindIndex(x => x.Name.Equals(fieldName, System.StringComparison.CurrentCultureIgnoreCase)) < 0)
            {
                m_MonoBehaviour.m_Script.TryGet(out var m_MonoScript);
                var assetName = m_MonoBehaviour.m_Name != "" ? m_MonoBehaviour.m_Name : m_MonoScript.m_ClassName;
                return null;
            }
            orderedDict = m_MonoBehaviour.ToType(m_Type);

            return orderedDict;
        }

        public static TypeTree ConvertToTypeTree(MonoBehavior m_MonoBehavior, AssemblyLoader assemblyLoader)
        {
            var m_Type = new TypeTree();
            var helper = new SerializedTypeHelper(m_MonoBehavior.Version);
            helper.AddMonoBehavior(m_Type.Nodes, 0);
            if (m_MonoBehavior.m_Script.TryGet(out var m_Script))
            {
                var typeDef = assemblyLoader.GetTypeDefinition(m_Script.m_AssemblyName, string.IsNullOrEmpty(m_Script.m_Namespace) ? m_Script.m_ClassName : $"{m_Script.m_Namespace}.{m_Script.m_ClassName}");
                if (typeDef != null)
                {
                    // var typeDefinitionConverter = new TypeDefinitionConverter(typeDef, helper, 1);
                    // m_Type.Nodes.AddRange(typeDefinitionConverter.ConvertToTypeTreeNodes());
                }
            }
            return m_Type;
        }
    }

    public enum CubismMonoBehaviorType
    {
        FadeMotionList,
        FadeMotion,
        Expression,
        Physics,
        DisplayInfo,
    }
}
