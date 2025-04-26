using System.Collections.Specialized;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class CubismExporter(int entryId, ISerializedFile resource) : IBundleExporter
    {
        public string FileName => resource[entryId].Name;

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (resource[entryId] is not MonoBehaviour behavior)
            {
                return;
            }
            if (!LocationStorage.TryCreate(fileName, ".moc3", mode, out fileName))
            {
                return;
            }
            var reader = resource.OpenRead(entryId);
            reader.Position = behavior.DataOffset;
            var length = reader.ReadUInt32();
            reader.ReadAsStream(length).SaveAs(fileName);
        }


        public static OrderedDictionary ParseMonoBehavior(int entryId, ISerializedFile resource,
            CubismMonoBehaviorType cubismMonoBehaviorType,
            AssemblyLoader assemblyLoader)
        {
            var orderedDict = UnityConverter.ToType(entryId, resource);
            if (orderedDict != null)
            {
                return orderedDict;
            }

            var fieldName = "";
            var behaviour = resource[entryId] as MonoBehaviour;
            var m_Type = BehaviorExporter.ConvertToTypeTree(behaviour, assemblyLoader, resource);
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
                behaviour.Script.TryGet(out var m_MonoScript);
                var assetName = behaviour.Name != "" ? behaviour.Name : m_MonoScript.ClassName;
                return null;
            }
            return UnityConverter.ToType(m_Type, entryId, resource);
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
