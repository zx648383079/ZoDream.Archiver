using System.Collections.Specialized;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class CubismExporter(MonoBehavior behavior) : IBundleExporter
    {
        public string Name => behavior.Name;

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, ".moc3", mode, out fileName))
            {
                return;
            }
            var reader = behavior.Reader;
            var length = reader.ReadUInt32();
            reader.ReadAsStream(length).SaveAs(fileName);
        }

        
        public void Dispose()
        {
        }

        public static OrderedDictionary ParseMonoBehavior(MonoBehavior m_MonoBehaviour,
            CubismMonoBehaviorType cubismMonoBehaviorType,
            AssemblyLoader assemblyLoader)
        {
            var orderedDict = m_MonoBehaviour.ToType();
            if (orderedDict != null)
            {
                return orderedDict;
            }

            var fieldName = "";
            var m_Type = BehaviorExporter.ConvertToTypeTree(m_MonoBehaviour, assemblyLoader);
            switch (cubismMonoBehaviorType)
            {
                case CubismMonoBehaviorType.FadeController:
                    fieldName = "cubismfademotionlist";
                    break;
                case CubismMonoBehaviorType.FadeMotionList:
                    fieldName = "cubismfademotionobjects";
                    break;
                case CubismMonoBehaviorType.FadeMotion:
                    fieldName = "parameterids";
                    break;
                case CubismMonoBehaviorType.ExpressionController:
                    fieldName = "expressionslist";
                    break;
                case CubismMonoBehaviorType.ExpressionList:
                    fieldName = "cubismexpressionobjects";
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
                case CubismMonoBehaviorType.PosePart:
                    fieldName = "groupindex";
                    break;
                case CubismMonoBehaviorType.Model:
                    fieldName = "_moc";
                    break;
                case CubismMonoBehaviorType.RenderTexture:
                    fieldName = "_maintexture";
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
    }

    public enum CubismMonoBehaviorType
    {
        FadeController,
        FadeMotionList,
        FadeMotion,
        ExpressionController,
        ExpressionList,
        Expression,
        Physics,
        DisplayInfo,
        PosePart,
        Model,
        RenderTexture,
    }
}
