using System.Collections.Specialized;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class CubismExporter(int entryId, ISerializedFile resource) : IMultipartExporter
    {
        public string FileName => resource[entryId].Name;

        public string SourcePath => resource.FullPath;

        public bool IsEmpty => false;

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
            switch (script.ClassName)
            {
                case "CubismMoc":
                case "CubismPhysicsController":
                case "CubismFadeController":
                case "CubismExpressionController":
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
            var reader = resource.OpenRead(entryId);
            reader.Position = behavior.DataOffset;
            var length = reader.ReadUInt32();
            reader.ReadAsStream(length).SaveAs(fileName);
        }

        public void Dispose()
        {
        }
        public static OrderedDictionary ParseMonoBehavior(int entryId, ISerializedFile resource
            AssemblyLoader assemblyLoader)
        {
            var orderedDict = UnityConverter.ToType(entryId, resource);
            if (orderedDict != null)
            {
                return orderedDict;
            }

            var behaviour = resource[entryId] as MonoBehaviour;
            var m_Type = BehaviorExporter.ConvertToTypeTree(behaviour, assemblyLoader, resource);
            behaviour.Script.TryGet(out var script);
            var fieldName = script.ClassName switch
            {
                "CubismFadeController" => "cubismfademotionlist",
                "CubismFadeMotionList" => "cubismfademotionobjects",
                "CubismFadeMotionData" => "parameterids",
                "CubismExpressionController" => "expressionslist",
                // "CubismExpressionController" => "cubismexpressionobjects",
                "CubismExpressionData" => "parameters",
                "CubismPhysicsController" => "_rig",
                "CubismDisplayInfoPartName" or "CubismDisplayInfoParameterName" => "name",
                "CubismPosePart" => "groupindex",
                "CubismModel" => "_moc",
                "CubismRenderer" => "_maintexture",
                "CubismEyeBlinkParameter" or "CubismMouthParameter" or "CubismParameter" or "CubismPart" => string.Empty,
                _ => string.Empty
            };
            if (m_Type.Nodes.FindIndex(x => x.Name.Equals(fieldName, System.StringComparison.CurrentCultureIgnoreCase)) < 0)
            {
                behaviour.Script.TryGet(out var m_MonoScript);
                var assetName = behaviour.Name != "" ? behaviour.Name : m_MonoScript.ClassName;
                return null;
            }
            return UnityConverter.ToType(m_Type, entryId, resource);
        }

        public static bool IsExportable(int entryId, ISerializedFile resource)
        {
            if (resource[entryId] is not GameObject game)
            {
                return false;
            }
            foreach (var item in GameObjectConverter.ForEach<MonoBehaviour>(game))
            {
                if (true == item.Script?.TryGet(out var script) && script.ClassName.StartsWith("Cubism"))
                {
                    return true;
                }
            }
            return false;
        }

     
    }
}
