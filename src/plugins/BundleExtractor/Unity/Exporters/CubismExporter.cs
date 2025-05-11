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
            _assembly = _resource.Container!.Assembly;
            _converter = new(resource);
            var obj = _resource[_entryId] as GameObject;
            Debug.Assert(obj is not null);
            Initialize(obj);
        }

        private readonly DocumentReader _converter;
        private readonly IAssemblyReader _assembly;
        private readonly int _entryId;
        private readonly ISerializedFile _resource;

        public bool IsEmpty => _moc is null;
        public string FileName { get; private set; }
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
            if (!GameObjectConverter.TryGet<Transform>(game, out var transform))
            {
                return;
            }
            transform = TransformConverter.GetRoot(transform);
            var items = new List<string>();
            foreach (var item in TransformConverter.ForEachTree(transform))
            {
                if (item.GameObject?.TryGet(out var obj) != true)
                {
                    continue;
                }
                _resource.AddExclude(item.GameObject.PathID);
                if (item == transform)
                {
                    FileName = obj.Name ?? string.Empty;
                }
                foreach (var pptr in obj.Components)
                {
                    if (!pptr.TryGet(out var instance))
                    {
                        continue;
                    }
                    if (instance is not MonoBehaviour behaviour)
                    {
                        continue;
                    }
                    if (!behaviour.Script.TryGet(out var script))
                    {
                        continue;
                    }
                    items.Add(script.ClassName);
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
                        case "CubismRenderer":
                            GetRenderTexture(_resource.IndexOf(pptr.PathID), behaviour);
                            break;
                        default:
                            break;
                    }
                }
            }
            
        }

        private void GetFadeList(int entryId, MonoBehaviour behaviour)
        {
            var data = ParseMonoBehavior(entryId);
            if (data is null)
            {
                return;
            }
            var res = _converter.ConvertType<IPPtr<MonoBehaviour>>(data[GetFieldName(behaviour)]);
            if (res?.IsNull != false)
            {
                return;
            }
            res.TryGet(out var obj); // CubismFadeMotionList
            obj?.Script.TryGet(out var script);
        }

        private void GetExpressionList(int entryId, MonoBehaviour behaviour)
        {
            var data = ParseMonoBehavior(entryId);
            if (data is null)
            {
                return;
            }
            var res = _converter.ConvertType<IPPtr<Object>>(data[GetFieldName(behaviour)]);
            if (res?.IsNull != false)
            {
                return;
            }
            res.TryGet(out var obj);
        }

        private void GetRenderTexture(int entryId, MonoBehaviour behaviour)
        {
            var data = ParseMonoBehavior(entryId);
            if (data is null)
            {
                return;
            }
            var res = data[GetFieldName(behaviour)];

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

        private OrderedDictionary? ParseMonoBehavior(int entryId)
        {
            var doc = _resource.GetType(entryId);
            if (doc is null)
            {
                if (_resource[entryId] is not MonoBehaviour behaviour)
                {
                    return null;
                }
                doc = BehaviorExporter.ConvertToTypeTree(behaviour, _assembly, _resource);
            }
            if (doc is null)
            {
                return null;
            }
            var res = _converter.Read(doc, _resource.OpenRead(entryId));
            if (res.Count == 1)
            {
                return (OrderedDictionary)res[0];
            }
            return res;
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
                "CubismFadeController" => "CubismFadeMotionList",
                "CubismFadeMotionList" => "CubismFadeMotionObjects",
                "CubismFadeMotionData" => "ParameterIds",
                "CubismExpressionController" => "ExpressionsList",
                "CubismExpressionList" => "CubismExpressionObjects",
                "CubismExpressionData" => "Parameters",
                "CubismPhysicsController" => "_rig",
                "CubismDisplayInfoPartName" or "CubismDisplayInfoParameterName" => "name",
                "CubismPosePart" => "GroupIndex",
                "CubismModel" => "_moc",
                "CubismRenderer" => "_mainTexture",
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
