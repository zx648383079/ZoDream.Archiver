using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
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
        private HashSet<string> _eyeBlinkParameters = [];
        private HashSet<string> _lipSyncParameters = [];
        private readonly HashSet<string> _parameterNames = [];
        private readonly HashSet<string> _partNames = [];
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
            if (!LocationStorage.TryCreate(baseFileName, ".model3.json", ArchiveExtractMode.Overwrite, out fileName))
            {
                return;
            }
            using var writer = JsonExporter.OpenWrite(fileName);
            writer.WriteStartObject();
            writer.WritePropertyName("Version");
            writer.WriteNumberValue(3);
            writer.WritePropertyName("Name");
            writer.WriteStringValue(FileName);
            writer.WritePropertyName("FileReferences");
            writer.WriteStartObject();
            if (_moc?.IsNotNull == true && LocationStorage.TryCreate(baseFileName, ".moc3", mode, out fileName))
            {
                var reader = _moc.Resource.OpenRead(_moc.Index);
                _moc.TryGet(out var behaviour);
                reader.Position = behaviour.DataOffset;
                var length = reader.ReadUInt32();
                reader.ReadAsStream(length).SaveAs(fileName);
                writer.WritePropertyName("Moc");
                writer.WriteStringValue(Path.GetFileName(fileName));
            }

            if (_textures.Count > 0)
            {
                writer.WritePropertyName("Textures");
                writer.WriteStringValue(Path.GetFileName(fileName));
            }
           

            if (_physics?.IsNotNull == true && LocationStorage.TryCreate(baseFileName, ".physics3.json", mode, out fileName))
            {
                using var sb = JsonExporter.OpenWrite(fileName);
                SavePhysics(sb);
                writer.WritePropertyName("Physics");
                writer.WriteStringValue(Path.GetFileName(fileName));
            }
            if (_poseParts.Count > 0 && LocationStorage.TryCreate(baseFileName, ".pose3.json", mode, out fileName))
            {
                writer.WritePropertyName("Pose");
                writer.WriteStringValue(Path.GetFileName(fileName));
            }
            if ((_parametersCdi.Count > 0 || _partsCdi.Count > 0)
                && LocationStorage.TryCreate(baseFileName, ".cdi3.json", mode, out fileName))
            {
                writer.WritePropertyName("DisplayInfo");
                writer.WriteStringValue(Path.GetFileName(fileName));
            }
            if (_motions.Count > 0)
            {
                writer.WritePropertyName("Motions");
                writer.WriteStringValue(Path.GetFileName(fileName));
            }
            if (_expressions.Count > 0)
            {
                writer.WritePropertyName("Expressions");
                writer.WriteStartArray();
                var childFolder = Path.Combine(folder, "expressions");
                foreach (var ptr in _expressions)
                {
                    if (!ptr.TryGet(out var behaviour))
                    {
                        continue;
                    }
                    if (!LocationStorage.TryCreate(Path.Combine(childFolder, behaviour.Name), ".exp3.json", mode, out fileName))
                    {
                        continue;
                    }
                    writer.WriteStartObject();
                    writer.WritePropertyName("Name");
                    writer.WritePropertyName("File");
                    writer.WriteStringValue($"expressions/{Path.GetFileName(fileName)}");
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
            writer.WritePropertyName("Groups");
            writer.WriteStartArray();

            if (_eyeBlinkParameters.Count == 0)
            {
                _eyeBlinkParameters = _parameterNames.Where(x =>
                    x.ToLower().Contains("eye")
                    && x.ToLower().Contains("open")
                    && (x.ToLower().Contains('l') || x.ToLower().Contains('r'))
                ).ToHashSet();
            }
            if (_lipSyncParameters.Count == 0)
            {
                _lipSyncParameters = _parameterNames.Where(x =>
                    x.ToLower().Contains("mouth")
                    && x.ToLower().Contains("open")
                    && x.ToLower().Contains('y')
                ).ToHashSet();
            }

            if (_eyeBlinkParameters.Count > 0)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("Target");
                writer.WriteStringValue("Parameter");
                writer.WritePropertyName("Name");
                writer.WriteStringValue("EyeBlink");
                writer.WritePropertyName("Ids");
                JsonExporter.Serialize(writer, _eyeBlinkParameters);
                writer.WriteEndObject();
            }
            if (_lipSyncParameters.Count > 0)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("Target");
                writer.WriteStringValue("Parameter");
                writer.WritePropertyName("Name");
                writer.WriteStringValue("LipSync");
                writer.WritePropertyName("Ids");
                JsonExporter.Serialize(writer, _lipSyncParameters);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        private void SavePhysics(Utf8JsonWriter writer)
        {
            if (ParseMonoBehavior(_physics)["_rig"] is not OrderedDictionary res)
            {
                return;
            }
            var target = "Parameter"; // 同名GameObject父节点的名称
            var subRigs = res["SubRigs"] as OrderedDictionary[];
            writer.WriteStartObject();
            writer.WritePropertyName("Version");
            writer.WriteNumberValue(3);
            writer.WritePropertyName("Meta");
            writer.WriteStartObject();
            writer.WritePropertyName("PhysicsSettingCount");
            writer.WriteNumberValue(subRigs.Length);
            writer.WritePropertyName("TotalInputCount");
            writer.WriteNumberValue(subRigs.Sum(x => (x["Input"] as object[]).Length));
            writer.WritePropertyName("TotalOutputCount");
            writer.WriteNumberValue(subRigs.Sum(x => (x["Output"] as object[]).Length));
            writer.WritePropertyName("VertexCount");
            writer.WriteNumberValue(subRigs.Sum(x => (x["Particles"] as object[]).Length));
            writer.WritePropertyName("Fps");
            JsonExporter.Serialize(writer, res["fps"]);
            writer.WritePropertyName("EffectiveForces");
            writer.WriteStartObject();
            writer.WritePropertyName("Gravity");
            JsonExporter.Serialize(writer, res["Gravity"]);
            writer.WritePropertyName("Wind");
            JsonExporter.Serialize(writer, res["Wind"]);
            writer.WriteEndObject();
            writer.WritePropertyName("PhysicsDictionary");
            writer.WriteStartArray();
            for (int i = 0; i < subRigs.Length; i++)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("Id");
                writer.WriteStringValue($"PhysicsSetting{i + 1}");
                writer.WritePropertyName("Name");
                writer.WriteStringValue($"Dummy{i + 1}");
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
            writer.WritePropertyName("PhysicsSettings");
            writer.WriteStartArray();
            for (int i = 0; i < subRigs.Length; i++)
            {
                var item = subRigs[i];
                writer.WriteStartObject();
                writer.WritePropertyName("Id");
                writer.WriteStringValue($"PhysicsSetting{i + 1}");
                writer.WritePropertyName("Input");
                writer.WriteStartArray();
                var items = item["Input"] as object[];
                for (int j = 0; j < items.Length; j++)
                {
                    var child = items[j] as OrderedDictionary;
                    writer.WriteStartObject();
                    writer.WritePropertyName("Source");
                    writer.WriteStartObject();
                    writer.WritePropertyName("Target");
                    writer.WriteStringValue(target);
                    writer.WritePropertyName("Id");
                    writer.WriteStringValue(child["SourceId"]?.ToString());
                    writer.WriteEndObject();
                    writer.WritePropertyName("Weight");
                    JsonExporter.Serialize(writer, child["Weight"]);
                    writer.WritePropertyName("Type");
                    writer.WriteStringValue(child["SourceComponent"]?.ToString());
                    writer.WritePropertyName("Reflect");
                    JsonExporter.Serialize(writer, child["IsInverted"]);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
                writer.WritePropertyName("Output");
                writer.WriteStartArray();
                items = item["Output"] as object[];
                for (int j = 0; j < items.Length; j++)
                {
                    var child = items[j] as OrderedDictionary;
                    writer.WriteStartObject();
                    writer.WritePropertyName("Destination");
                    writer.WriteStartObject();
                    writer.WritePropertyName("Target");
                    JsonExporter.Serialize(writer, target);
                    writer.WritePropertyName("Id");
                    writer.WriteStringValue(child["DestinationId"]?.ToString());
                   writer.WriteEndObject();
                    writer.WritePropertyName("VertexIndex");
                    JsonExporter.Serialize(writer, child["ParticleIndex"]);
                    writer.WritePropertyName("Scale");
                    JsonExporter.Serialize(writer, child["AngleScale"]);
                    writer.WritePropertyName("Weight");
                    JsonExporter.Serialize(writer, child["Weight"]);
                    writer.WritePropertyName("Type");
                    writer.WriteStringValue(child["SourceComponent"]?.ToString());
                    writer.WritePropertyName("Reflect");
                    JsonExporter.Serialize(writer, child["IsInverted"]);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
                writer.WritePropertyName("Vertices");
                writer.WriteStartArray();
                items = item["Particles"] as object[];
                for (int j = 0; j < items.Length; j++)
                {
                    var child = items[j] as OrderedDictionary;
                    writer.WriteStartObject();
                    writer.WritePropertyName("Position");
                    JsonExporter.Serialize(writer, child["InitialPosition"]);
                    writer.WritePropertyName("Mobility");
                    JsonExporter.Serialize(writer, child["Mobility"]);
                    writer.WritePropertyName("Delay");
                    JsonExporter.Serialize(writer, child["Delay"]);
                    writer.WritePropertyName("Acceleration");
                    JsonExporter.Serialize(writer, child["Acceleration"]);
                    writer.WritePropertyName("Radius");
                    JsonExporter.Serialize(writer, child["Radius"]);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
                writer.WritePropertyName("Normalization");
                JsonExporter.Serialize(writer, item["Normalization"]);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
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
