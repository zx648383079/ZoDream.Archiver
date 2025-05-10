using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.BundleExtractor.Unity.Document;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class CubismExporter : IMultipartExporter
    {
        public CubismExporter(int entryId, ISerializedFile resource)
        {
            _entryId = entryId;
            _resource = resource;
            var obj = _resource[_entryId] as GameObject;
            Debug.Assert(obj is not null);
            Initialize(obj);
        }

        private readonly int _entryId;
        private readonly ISerializedFile _resource;

        public bool IsEmpty => _moc is null;
        public string FileName => _resource[_entryId].Name;
        public string SourcePath => _resource.FullPath;

        private MonoBehaviour? _moc;
        private MonoBehaviour? _model;
        private MonoBehaviour? _pose;
        private MonoBehaviour? _physics;
        private MonoBehaviour? _displayInfo;
        private MonoBehaviour? _userData;
        private List<MonoBehaviour> _motions = [];
        private List<MonoBehaviour> _expressions = [];
        private List<Texture2D> _textures = [];

        private void Initialize(GameObject game)
        {
            foreach (var pptr in game.Components)
            {
                if (!pptr.TryGet(out var instance) || instance is not MonoBehaviour behaviour)
                {
                    continue;
                }
                if (!behaviour.Script.TryGet(out var script))
                {
                    continue;
                }
                switch (script.ClassName)
                {
                    case "CubismModel":
                        _model = behaviour;
                        break;
                    case "CubismPhysicsController":
                        _physics = behaviour;
                        break;
                    case "CubismFadeController":
                        GetFadeList(_resource.IndexOf(pptr.PathID), behaviour);
                        break;
                    case "CubismExpressionController":
                        GetExpressionList(_resource.IndexOf(pptr.PathID), behaviour);
                        break;
                    default:
                        break;
                }
            }
        }

        private void GetFadeList(int entryId, MonoBehaviour behaviour)
        {
            var data = ParseMonoBehavior(entryId, _resource, _resource.Container.Service.Get<AssemblyLoader>())
        }

        private void GetExpressionList(int entryId, MonoBehaviour behaviour)
        {

        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (_resource[_entryId] is not MonoBehaviour behavior || !behavior.Script.TryGet(out var script))
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
            var reader = _resource.OpenRead(_entryId);
            reader.Position = behavior.DataOffset;
            var length = reader.ReadUInt32();
            reader.ReadAsStream(length).SaveAs(fileName);
        }




        public void Dispose()
        {
        }

        public static OrderedDictionary? ParseMonoBehavior(int entryId, ISerializedFile resource,
            AssemblyLoader assemblyLoader)
        {
            var orderedDict = UnityConverter.ToType(entryId, resource);
            if (orderedDict != null)
            {
                return orderedDict;
            }
            if (resource[entryId] is not MonoBehaviour behaviour)
            {
                return null;
            }
            var res = BehaviorExporter.ConvertToTypeTree(behaviour, assemblyLoader, resource);
            return UnityConverter.ToType(res, entryId, resource);
        }

        private static string GetFieldName(MonoBehaviour behaviour)
        {
            if (behaviour.Script.TryGet(out var script))
            {
                return GetFieldName(script);
            }
            return string.Empty;
        }

        private static string GetFieldName(MonoScript script)
        {
            return script.ClassName switch
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
        }

        internal static void ConvertToTypeTree(DocumentBuilder helper, MonoScript script)
        {
            switch (script.ClassName)
            {
                case "CubismModel":
                    helper.AddMonoCubismModel(1);
                    break;
                case "CubismMoc":
                    helper.AddMonoCubismMoc(1);
                    break;
                case "CubismFadeController":
                    helper.AddMonoCubismFadeController(1);
                    break;
                case "CubismFadeMotionList":
                    helper.AddMonoCubismFadeList(1);
                    break;
                case "CubismFadeMotionData":
                    helper.AddMonoCubismFadeData(1);
                    break;
                case "CubismExpressionController":
                    helper.AddMonoCubismExpressionController(1);
                    break;
                case "CubismExpressionList":
                    helper.AddMonoCubismExpressionList(1);
                    break;
                case "CubismExpressionData":
                    helper.AddMonoCubismExpressionData(1);
                    break;
                case "CubismDisplayInfoParameterName":
                    helper.AddMonoCubismDisplayInfo(1);
                    break;
                case "CubismDisplayInfoPartName":
                    helper.AddMonoCubismDisplayInfo(1);
                    break;
                case "CubismPosePart":
                    helper.AddMonoCubismPosePart(1);
                    break;
            }
        }
        public static bool IsExportable(int entryId, ISerializedFile resource)
        {
            if (resource[entryId] is not GameObject game)
            {
                return false;
            }
            foreach (var item in GameObjectConverter.ForEach<MonoBehaviour>(game))
            {
                if (true == item.Script?.TryGet(out var script) 
                    && script.ClassName.StartsWith("Cubism"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
