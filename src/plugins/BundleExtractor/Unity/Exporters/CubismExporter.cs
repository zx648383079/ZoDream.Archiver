using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.BundleExtractor.Unity.Document;
using ZoDream.Shared.IO;
using ZoDream.Shared.Language;
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
            FileName = obj.Name ?? string.Empty;
            Initialize(obj);
        }

        private readonly DocumentReader _converter;
        private readonly IAssemblyReader _assembly;
        private readonly int _entryId;
        private readonly ISerializedFile _resource;

        public bool IsEmpty => _moc is null;
        public string FileName { get; private set; }
        public string SourcePath => _resource.FullPath;

        private IPPtr<MonoBehaviour>? _moc;
        private IPPtr<MonoBehaviour>? _physics;
        private readonly List<IPPtr<MonoBehaviour>> _poseParts = [];
        private readonly List<IPPtr<MonoBehaviour>> _parametersCdi = [];
        private readonly List<string> _eyeBlinkParameters = [];
        private readonly List<string> _lipSyncParameters = [];
        private readonly List<string> _parameterNames = [];
        private readonly List<string> _partNames = [];
        private readonly List<IPPtr<MonoBehaviour>> _partsCdi = [];
        private readonly List<IPPtr<MonoBehaviour>> _motions = [];
        private readonly List<IPPtr<MonoBehaviour>> _expressions = [];
        private readonly List<IPPtr<Texture2D>> _textures = [];

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
                    items.Add($"[{script.ClassName}]{behaviour.Name}-{script.Name}");
                    switch (script.ClassName)
                    {
                        case "CubismModel":
                            GetModel(pptr.Create<MonoBehaviour>(pptr));
                            break;
                        case "CubismPhysicsController":
                            _physics = pptr.Create<MonoBehaviour>(pptr);
                            break;
                        case "CubismFadeController":
                            GetFadeController(pptr.Create<MonoBehaviour>(pptr));
                            break;
                        case "CubismExpressionController":
                            GetExpressionList(pptr.Create<MonoBehaviour>(pptr));
                            break;
                        case "CubismRenderer":
                            GetRenderTexture(pptr.Create<MonoBehaviour>(pptr));
                            break;
                        case "CubismExpressionData":
                            _expressions.Add(pptr.Create<MonoBehaviour>(pptr));
                            break;
                        case "CubismFadeMotionData":
                            _motions.Add(pptr.Create<MonoBehaviour>(pptr));
                            break;
                        case "CubismFadeMotionList":
                            GetFadeList(pptr.Create<MonoBehaviour>(pptr));
                            break;
                        case "CubismEyeBlinkParameter":
                            _eyeBlinkParameters.Add(obj.Name);
                            break;
                        case "CubismMouthParameter":
                            _lipSyncParameters.Add(obj.Name);
                            break;
                        case "CubismParameter":
                            _parameterNames.Add(obj.Name);
                            break;
                        case "CubismPart":
                            _partNames.Add(obj.Name);
                            break;
                        case "CubismPosePart":
                            _poseParts.Add(pptr.Create<MonoBehaviour>(pptr));
                            break;
                        case "CubismDisplayInfoParameterName":
                            _parametersCdi.Add(pptr.Create<MonoBehaviour>(pptr));
                            break;
                        case "CubismDisplayInfoPartName":
                            _partsCdi.Add(pptr.Create<MonoBehaviour>(pptr));
                            break;
                        default:
                            break;
                    }
                }
            }
            
        }

        private void GetModel(IPPtr<MonoBehaviour> ptr)
        {
            var res = ParseMonoBehavior<MonoBehaviour>(ptr);
            if (res is null)
            {
                return;
            }
            _moc = res;
        }

        

        private void GetFadeController(IPPtr<MonoBehaviour> ptr)
        {
            var pptr = ParseMonoBehavior<MonoBehaviour>(ptr);
            if (pptr is null)
            {
                return;
            }
            GetFadeList(pptr);
        }

        private void GetFadeList(IPPtr<MonoBehaviour> ptr)
        {
            var data = ParseMonoBehavior(ptr); // CubismFadeMotionList
            if (data["CubismFadeMotionObjects"] is not object[] items)
            {
                return;
            }
            foreach (var item in items)
            {
                _motions.Add(_converter.ConvertType<IPPtr<MonoBehaviour>>(item));
            }
        }

        private void GetExpressionList(IPPtr<MonoBehaviour> ptr)
        {
            var pptr = ParseMonoBehavior<MonoBehaviour>(ptr);
            if (pptr is null)
            {
                return;
            }
            var data = ParseMonoBehavior(pptr);
            if (data["CubismExpressionObjects"] is not object[] items)
            {
                return;
            }
            foreach (var item in items)
            {
                _expressions.Add(_converter.ConvertType<IPPtr<MonoBehaviour>>(item));
            }
        }

        private void GetRenderTexture(IPPtr<MonoBehaviour> ptr)
        {
            var res = ParseMonoBehavior<Texture2D>(ptr);
            if (res is null)
            {
                return;
            }
            // Texture2D
            if (!_textures.Contains(res))
            {
                _textures.Add(res);
            }
            
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (IsEmpty)
            {
                return;
            }
            var folder = fileName;
            var baseFileName = Path.Combine(folder, FileName);
            if (_moc?.IsNotNull == true && !LocationStorage.TryCreate(baseFileName, ".moc3", mode, out fileName))
            {
                var reader = _moc.Resource.OpenRead(_moc.Index);
                _moc.TryGet(out var behaviour);
                reader.Position = behaviour.DataOffset;
                var length = reader.ReadUInt32();
                reader.ReadAsStream(length).SaveAs(fileName);
            }
            if (_physics?.IsNotNull == true && !LocationStorage.TryCreate(baseFileName, ".physics3.json", mode, out fileName))
            {
                using var sb = new CodeWriter(File.Create(baseFileName));
                SavePhysics(sb);
            }
        }

        private void SavePhysics(ICodeWriter writer)
        {
            if (ParseMonoBehavior(_physics)["_rig"] is not OrderedDictionary res)
            {
                return;
            }
            var target = "Parameter"; // 同名GameObject父节点的名称
            var subRigs = res["SubRigs"] as OrderedDictionary[];
            writer.Write('{')
                .Write("Version", true).Write(":3,")
                .Write("Meta", true).Write(":{")
                .Write("PhysicsSettingCount", true).Write(':').Write(subRigs.Length).Write(',')
                .Write("TotalInputCount", true).Write(':').Write(subRigs.Sum(x => (x["Input"] as object[]).Length)).Write(',')
                .Write("TotalOutputCount", true).Write(':').Write(subRigs.Sum(x => (x["Output"] as object[]).Length)).Write(',')
                .Write("VertexCount", true).Write(':').Write(subRigs.Sum(x => (x["Particles"] as object[]).Length)).Write(',')
                .Write("Fps", true).Write(':').Write(res["fps"]).Write(',')
                .Write("EffectiveForces", true).Write(":{")
                    .Write("Gravity", true).Write(':');
            Write(writer, res["Gravity"] as OrderedDictionary);
            writer.Write(',')
                .Write("Wind", true).Write(':');
            Write(writer, res["Wind"] as OrderedDictionary);
            writer.Write("},")
                .Write("PhysicsDictionary", true).Write(":[");
            for (int i = 0; i < subRigs.Length; i++)
            {
                if (i > 0)
                {
                    writer.Write(',');
                }
                writer.Write('{')
                    .Write("Id", true).Write(':').Write($"PhysicsSetting{i + 1}", true).Write(',')
                    .Write("Name", true).Write(':').Write($"Dummy{i + 1}", true)
                    .Write('}');
            }
            writer.Write("]},")
                .Write("PhysicsSettings", true).Write(":[");
            for (int i = 0; i < subRigs.Length; i++)
            {
                var item = subRigs[i];
                if (i > 0)
                {
                    writer.Write(',');
                }
                writer.Write('{')
                    .Write("Id", true).Write(':').Write($"PhysicsSetting{i + 1}", true).Write(',');
                writer.Write("Input", true).Write(":[");
                var items = item["Input"] as object[];
                for (int j = 0; j < items.Length; j++)
                {
                    var child = items[j] as OrderedDictionary;
                    if (j > 0)
                    {
                        writer.Write(',');
                    }
                    writer.Write('{')
                        .Write("Source", true).Write(":{")
                            .Write("Target", true).Write(':').Write(target, true).Write(',')
                            .Write("Id", true).Write(':').Write(child["SourceId"].ToString(), true)
                        .Write("},")
                        .Write("Weight", true).Write(':').Write(child["Weight"]).Write(',')
                        .Write("Type", true).Write(':').Write(child["SourceComponent"].ToString(), true).Write(',')
                        .Write("Reflect", true).Write(':').Write(child["IsInverted"])
                        .Write('}');
                }
                writer.Write("],");
                writer.Write("Output", true).Write(":[");
                items = item["Output"] as object[];
                for (int j = 0; j < items.Length; j++)
                {
                    var child = items[j] as OrderedDictionary;
                    if (j > 0)
                    {
                        writer.Write(',');
                    }
                    writer.Write('{')
                        .Write("Destination", true).Write(":{")
                            .Write("Target", true).Write(':').Write(target, true).Write(',')
                            .Write("Id", true).Write(':').Write(child["DestinationId"].ToString(), true)
                        .Write("},")
                        .Write("VertexIndex", true).Write(':').Write(child["ParticleIndex"]).Write(',')
                        .Write("Scale", true).Write(':').Write(child["AngleScale"]).Write(',')
                        .Write("Weight", true).Write(':').Write(child["Weight"]).Write(',')
                        .Write("Type", true).Write(':').Write(child["SourceComponent"].ToString(), true).Write(',')
                        .Write("Reflect", true).Write(':').Write(child["IsInverted"])
                        .Write('}');
                }
                writer.Write("],");
                writer.Write("Vertices", true).Write(":[");
                items = item["Particles"] as object[];
                for (int j = 0; j < items.Length; j++)
                {
                    var child = items[j] as OrderedDictionary;
                    if (j > 0)
                    {
                        writer.Write(',');
                    }
                    writer.Write('{')
                        .Write("Position", true).Write(':');
                    Write(writer, child["InitialPosition"] as OrderedDictionary);
                    writer.Write(',')
                        .Write("Mobility", true).Write(':').Write(child["Mobility"]).Write(',')
                        .Write("Delay", true).Write(':').Write(child["Delay"]).Write(',')
                        .Write("Acceleration", true).Write(':').Write(child["Acceleration"]).Write(',')
                        .Write("Radius", true).Write(':').Write(child["Radius"])
                        .Write('}');
                }
                writer.Write("],");
                writer.Write("Normalization", true).Write(':');
                Write(writer, item["Normalization"] as OrderedDictionary);
                writer.Write('}');
            }
            writer.Write("]}");
        }

        private static void Write(ICodeWriter writer, OrderedDictionary data)
        {
            writer.Write('{');
            var i = 0;
            foreach (var key in data.Keys)
            {
                if (i++ > 0)
                {
                    writer.Write(',');
                }
                writer.Write(key.ToString(), true).Write(':');
                if (data[key] is OrderedDictionary next)
                {
                    Write(writer, next);
                } else
                {
                    writer.Write(data[key]);
                }
            }
            writer.Write('}');
        }

        public void Dispose()
        {
        }

        private IPPtr<T>? ParseMonoBehavior<T>(IPPtr<MonoBehaviour> ptr)
            where T : Object
        {
            var data = ParseMonoBehavior(ptr);
            if (data is null || !ptr.TryGet(out var behaviour))
            {
                return null;
            }
            var fieldName = GetFieldName(behaviour);
            if (data is null || !data.Contains(fieldName))
            {
                return null;
            }
            var res = _converter.ConvertType<IPPtr<T>>(data[fieldName]!);
            if (res?.IsNotNull != true)
            {
                return null;
            }
            return res;
        }

        private OrderedDictionary? ParseMonoBehavior(IPPtr<MonoBehaviour> ptr)
        {
            var doc = ptr.Resource.GetType(ptr.Index);
            if (doc is null)
            {
                if (_resource[ptr.Index] is not MonoBehaviour behaviour)
                {
                    return null;
                }
                doc = BehaviorExporter.ConvertToTypeTree(behaviour, _assembly, _resource);
            }
            if (doc is null)
            {
                return null;
            }
            return _converter.Read(doc, ptr.Resource.OpenRead(ptr.Index));
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
