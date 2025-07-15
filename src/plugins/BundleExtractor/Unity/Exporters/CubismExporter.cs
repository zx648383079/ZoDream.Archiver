using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.BundleExtractor.Unity.Document;
using ZoDream.BundleExtractor.Unity.Live2d;
using ZoDream.Shared;
using ZoDream.Shared.Drawing;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;
using Object = UnityEngine.Object;

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
            Expectation.ThrowIfNot(obj is not null);
            FileName = obj.Name ?? string.Empty;
            Initialize(obj);
        }
        public CubismExporter(IPPtr<GameObject> ptr)
            : this(ptr.Index, (ISerializedFile)ptr.Resource)
        {
        }

        private readonly DocumentReader _converter;
        private readonly IAssemblyReader _assembly;
        private readonly int _entryId;
        private readonly ISerializedFile _resource;

        public bool IsEmpty => _moc is null;
        public string FileName { get; private set; }
        public IFilePath SourcePath => _resource.FullPath;

        private IPPtr<MonoBehaviour>? _moc;
        private IPPtr<MonoBehaviour>? _physics;
        private readonly HashSet<IPPtr<MonoBehaviour>> _poseParts = [];
        private readonly HashSet<IPPtr<MonoBehaviour>> _parametersCdi = [];
        private HashSet<string> _eyeBlinkParameters = [];
        private HashSet<string> _lipSyncParameters = [];
        private readonly HashSet<string> _parameterNames = [];
        private readonly HashSet<string> _partNames = [];
        private readonly HashSet<IPPtr<MonoBehaviour>> _partsCdi = [];
        private readonly HashSet<IPPtr<MonoBehaviour>> _motions = [];
        private readonly HashSet<IPPtr<MonoBehaviour>> _expressions = [];
        private readonly HashSet<IPPtr<Texture2D>> _textures = [];

        private void Initialize(GameObject game)
        {
            if (!GameObjectConverter.TryGet<Transform>(game, out var transform))
            {
                return;
            }
            transform = TransformConverter.GetRoot(transform);
            foreach (var item in TransformConverter.ForEachTree(transform))
            {
                if (item.GameObject?.TryGet(out var obj) != true)
                {
                    continue;
                }
                item.GameObject.IsExclude = true;
                if (item == transform)
                {
                    FileName = obj.Name ?? string.Empty;
                }
                foreach (var pptr in obj.Components)
                {
                    pptr.IsExclude = true;
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
            var res = GetChildToPPtr<MonoBehaviour>(ptr);
            if (res is null)
            {
                return;
            }
            _moc = res;
        }

        private void GetFadeController(IPPtr<MonoBehaviour> ptr)
        {
            var pptr = GetChildToPPtr<MonoBehaviour>(ptr);
            if (pptr is null)
            {
                return;
            }
            GetFadeList(pptr);
        }

        private void GetFadeList(IPPtr<MonoBehaviour> ptr)
        {
            var data = BehaviorExporter.Deserialize(ptr, _assembly, _converter); // CubismFadeMotionList
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
            var pptr = GetChildToPPtr<MonoBehaviour>(ptr);
            if (pptr is null)
            {
                return;
            }
            var data = BehaviorExporter.Deserialize(pptr, _assembly, _converter);
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
            var res = GetChildToPPtr<Texture2D>(ptr);
            if (res is null)
            {
                return;
            }
            // Texture2D
            _textures.Add(res);
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
                writer.WriteStartArray();
                var childFolder = Path.Combine(folder, "textures");
                foreach (var ptr in _textures)
                {
                    if (!ptr.TryGet(out var texture))
                    {
                        continue;
                    }
                    if (!LocationStorage.TryCreate(Path.Combine(childFolder, texture.Name), ".png", mode, out fileName))
                    {
                        continue;
                    }
                    using var image = TextureExporter.ToImage(texture, ptr.Resource, true);
                    if (image is null)
                    {
                        continue;
                    }
                    image.SaveAs(fileName);
                    writer.WriteStringValue($"textures/{Path.GetFileName(fileName)}");
                }
                writer.WriteEndArray();
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
                using var sb = JsonExporter.OpenWrite(fileName);
                SavePose(sb);
                writer.WritePropertyName("Pose");
                writer.WriteStringValue(Path.GetFileName(fileName));
            }
            if ((_parametersCdi.Count > 0 || _partsCdi.Count > 0)
                && LocationStorage.TryCreate(baseFileName, ".cdi3.json", mode, out fileName))
            {
                using var sb = JsonExporter.OpenWrite(fileName);
                SavCdi(sb);
                writer.WritePropertyName("DisplayInfo");
                writer.WriteStringValue(Path.GetFileName(fileName));
            }
            if (_motions.Count > 0)
            {
                var childFolder = Path.Combine(folder, "motions");
                var maps = new SortedDictionary<string, List<string>>();
                foreach (var ptr in _motions)
                {
                    if (!ptr.TryGet(out var behaviour))
                    {
                        continue;
                    }
                    var data = BehaviorExporter.Deserialize<CubismFadeMotionData>(ptr, _assembly, _converter);
                    if (data is null || !LocationStorage.TryCreate(Path.Combine(childFolder, behaviour.Name), ".motion3.json", mode, out fileName))
                    {
                        continue;
                    }
                    using var sb = JsonExporter.OpenWrite(fileName);
                    SaveMotion(sb, data);
                    if (maps.TryGetValue(behaviour.Name, out List<string>? value))
                    {
                        value.Add(Path.GetFileName(fileName));
                    }
                    else
                    {
                        maps.Add(behaviour.Name, [Path.GetFileName(fileName)]);
                    }
                }
                writer.WritePropertyName("Motions");
                writer.WriteStartObject();
                foreach (var items in maps)
                {
                    writer.WritePropertyName(items.Key);
                    writer.WriteStartArray();
                    foreach (var item in items.Value)
                    {
                        writer.WriteStartObject();
                        writer.WritePropertyName("File");
                        writer.WriteStringValue($"motions/{item}");
                        writer.WriteEndObject();
                    }
                    writer.WriteEndArray();
                }
                writer.WriteEndObject();
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
                    var data = BehaviorExporter.Deserialize<CubismExpressionData>(ptr, _assembly, _converter);
                    if (data is null || !LocationStorage.TryCreate(Path.Combine(childFolder, behaviour.Name), ".exp3.json", mode, out fileName))
                    {
                        continue;
                    }
                    using var sb = JsonExporter.OpenWrite(fileName);
                    SaveExpression(sb, data);
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

        private void SaveMotion(Utf8JsonWriter writer, CubismFadeMotionData data)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Version");
            writer.WriteNumberValue(3);


            writer.WritePropertyName("Curves");
            writer.WriteStartArray();

            var curveCount = 0;
            var totalSegmentCount = 0;
            var totalPointCount = 0;
            var i = -1;
            foreach (var item in data.ParameterCurves)
            {
                i++;
                if (item is null)
                {
                    continue;
                }
                var curveItems = item.Curve;
                if (curveItems.Length == 0)
                {
                    continue;
                }
                string target;
                var paramId = data.ParameterIds[i];
                switch (paramId)
                {
                    case "Opacity":
                    case "EyeBlink":
                    case "LipSync":
                        target = "Model";
                        break;
                    default:
                        if (_parameterNames.Contains(paramId))
                        {
                            target = "Parameter";
                        }
                        else if (_partNames.Contains(paramId))
                        {
                            target = "PartOpacity";
                        }
                        else
                        {
                            target = paramId.ToLower().Contains("part") ? "PartOpacity" : "Parameter";
                            _resource.Logger?.Warning($"[{data.Name}] Binding error: Unable to find \"{paramId}\" among the model parts/parameters");
                        }
                        break;
                }
                writer.WriteStartObject();
                writer.WritePropertyName("Target");
                writer.WriteStringValue(target);
                writer.WritePropertyName("Id");
                writer.WriteStringValue(paramId);
                writer.WritePropertyName("FadeInTime");
                writer.WriteNumberValue(data.ParameterFadeInTimes[i]);
                writer.WritePropertyName("FadeOutTime");
                writer.WriteNumberValue(data.ParameterFadeOutTimes[i]);
                writer.WritePropertyName("Segments");
                writer.WriteStartArray();
                for (var j = 1; j < curveItems.Length; j++)
                {
                    var curve = curveItems[j];
                    var preCurve = curveItems[j - 1];
                    var nextCurve = curveItems.ElementAtOrDefault(j + 1);
                    if (Math.Abs(curve.Time - preCurve.Time - 0.01f) < 0.0001f) // InverseSteppedSegment
                    {
                        if (nextCurve.Value == curve.Value)
                        {
                            writer.WriteNumberValue(3f); // Segment ID
                            writer.WriteNumberValue(nextCurve.Time);
                            writer.WriteNumberValue(nextCurve.Value);
                            j += 1;
                            totalPointCount += 1;
                            totalSegmentCount++;
                            continue;
                        }
                    }
                    if (float.IsPositiveInfinity(curve.InSlope)) // SteppedSegment
                    {
                        writer.WriteNumberValue(2f); // Segment ID
                        writer.WriteNumberValue(curve.Time);
                        writer.WriteNumberValue(curve.Value);
                        totalPointCount += 1;
                    }
                    else if (preCurve.OutSlope == 0f && Math.Abs(curve.InSlope) < 0.0001f) // LinearSegment
                    {
                        writer.WriteNumberValue(0f); // Segment ID
                        writer.WriteNumberValue(curve.Time);
                        writer.WriteNumberValue(curve.Value);
                        totalPointCount += 1;
                    }
                    else // BezierSegment
                    {
                        var tangentLength = (curve.Time - preCurve.Time) / 3f;
                        writer.WriteNumberValue(1f); // Segment ID
                        writer.WriteNumberValue(preCurve.Time + tangentLength);
                        writer.WriteNumberValue(preCurve.OutSlope * tangentLength + (float)preCurve.Value);
                        writer.WriteNumberValue(curve.Time - tangentLength);
                        writer.WriteNumberValue(curve.Value - (float)curve.InSlope * tangentLength);
                        writer.WriteNumberValue(curve.Time);
                        writer.WriteNumberValue(curve.Value);
                        totalPointCount += 3;
                    }
                    totalSegmentCount++;
                }
                curveCount++;
                totalPointCount++;
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            writer.WriteEndArray();


            writer.WritePropertyName("Meta");

            writer.WriteStartObject();
            writer.WritePropertyName("Duration");
            writer.WriteNumberValue(data.MotionLength);
            writer.WritePropertyName("Fps");
            writer.WriteNumberValue(30);
            writer.WritePropertyName("Loop");
            writer.WriteBooleanValue(true);
            writer.WritePropertyName("AreBeziersRestricted");
            writer.WriteBooleanValue(true);
            writer.WritePropertyName("FadeInTime");
            writer.WriteNumberValue(data.FadeInTime);
            writer.WritePropertyName("FadeOutTime");
            writer.WriteNumberValue(data.FadeOutTime);
            writer.WritePropertyName("UserDataCount");
            writer.WriteNumberValue(0);
            writer.WritePropertyName("CurveCount");
            writer.WriteNumberValue(curveCount);
            writer.WritePropertyName("TotalSegmentCount");
            writer.WriteNumberValue(totalSegmentCount);
            writer.WritePropertyName("TotalPointCount");
            writer.WriteNumberValue(totalPointCount);
            writer.WritePropertyName("TotalUserDataSize");
            writer.WriteNumberValue(0);
            writer.WriteEndObject();



            writer.WritePropertyName("UserData");
            writer.WriteStartArray();
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        private void SaveExpression(Utf8JsonWriter writer, CubismExpressionData data)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Type");
            writer.WriteStringValue(data.Type);
            writer.WritePropertyName("FadeInTime");
            writer.WriteNumberValue(data.FadeInTime);
            writer.WritePropertyName("FadeOutTime");
            writer.WriteNumberValue(data.FadeOutTime);
            writer.WritePropertyName("Parameters");
            writer.WriteStartArray();
            foreach (var item in data.Parameters)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("Id");
                writer.WriteStringValue(item.Id);
                writer.WritePropertyName("Value");
                writer.WriteNumberValue(item.Value);
                writer.WritePropertyName("Blend");
                writer.WriteStringValue(Enum.GetName(item.Blend));
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
        }

        private void SavCdi(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Version");
            writer.WriteNumberValue(3);
            writer.WritePropertyName("ParameterGroups");
            writer.WriteStartArray();
            writer.WriteEndArray();

            writer.WritePropertyName("Parameters");
            writer.WriteStartArray();
            foreach (var ptr in _parametersCdi)
            {
                if (!ptr.TryGet(out var behaviour) || !behaviour.GameObject.TryGet(out var game))
                {
                    continue;
                }
                var displayName = GetDisplayName(ptr);
                if (displayName is null)
                {
                    continue;
                }
                writer.WriteStartObject();
                writer.WritePropertyName("Id");
                writer.WriteStringValue(game.Name);
                writer.WritePropertyName("GroupId");
                writer.WriteStringValue(string.Empty);
                writer.WritePropertyName("Name");
                writer.WriteStringValue(displayName);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WritePropertyName("Parts");
            writer.WriteStartArray();
            foreach (var ptr in _partsCdi)
            {
                if (!ptr.TryGet(out var behaviour) || !behaviour.GameObject.TryGet(out var game))
                {
                    continue;
                }
                var displayName = GetDisplayName(ptr);
                if (displayName is null)
                {
                    continue;
                }
                writer.WriteStartObject();
                writer.WritePropertyName("Id");
                writer.WriteStringValue(game.Name);
                writer.WritePropertyName("Name");
                writer.WriteStringValue(displayName);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        private void SavePose(Utf8JsonWriter writer)
        {
            var groupDict = new SortedDictionary<int, List<KeyValuePair<string, string[]>>>();
            foreach (var ptr in _poseParts)
            {
                if (!ptr.TryGet(out var behaviour) || !behaviour.GameObject.TryGet(out var game))
                {
                    continue;
                }
                var data = BehaviorExporter.Deserialize(ptr, _assembly, _converter);
                if (data is null)
                {
                    continue;
                }
                var node = new KeyValuePair<string, string[]>(game.Name, 
                    Array.ConvertAll((object[])data["Link"], x => x?.ToString()));
                var groupIndex = (int)data["GroupIndex"];
                if (groupDict.ContainsKey(groupIndex))
                {
                    groupDict[groupIndex].Add(node);
                }
                else
                {
                    groupDict.Add(groupIndex, [node]);
                }
            }

            writer.WriteStartObject();
            writer.WritePropertyName("Type");
            writer.WriteStringValue("Live2D Pose");
            writer.WritePropertyName("Groups");
            writer.WriteStartArray();
            foreach (var items in groupDict.Values)
            {
                writer.WriteStartArray();
                foreach (var item in items)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Id");
                    writer.WriteStringValue(item.Key);
                    writer.WritePropertyName("Link");
                    JsonExporter.Serialize(writer, item.Value);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        private void SavePhysics(Utf8JsonWriter writer)
        {
            var res = BehaviorExporter.Deserialize<CubismPhysics>(_physics, _assembly, _converter)?.Rig;
            if (res is null)
            {
                return;
            }
            var target = "Parameter"; // 同名GameObject父节点的名称
            var subRigs = res.SubRigs;
            writer.WriteStartObject();
            writer.WritePropertyName("Version");
            writer.WriteNumberValue(3);
            writer.WritePropertyName("Meta");
            writer.WriteStartObject();
            writer.WritePropertyName("PhysicsSettingCount");
            writer.WriteNumberValue(subRigs.Length);
            writer.WritePropertyName("TotalInputCount");
            writer.WriteNumberValue(subRigs.Sum(x => x.Input.Length));
            writer.WritePropertyName("TotalOutputCount");
            writer.WriteNumberValue(subRigs.Sum(x => x.Output.Length));
            writer.WritePropertyName("VertexCount");
            writer.WriteNumberValue(subRigs.Sum(x => x.Particles.Length));
            writer.WritePropertyName("Fps");
            writer.WriteNumberValue(res.Fps);
            writer.WritePropertyName("EffectiveForces");
            writer.WriteStartObject();
            writer.WritePropertyName("Gravity");
            JsonExporter.Serialize(writer, res.Gravity);
            writer.WritePropertyName("Wind");
            JsonExporter.Serialize(writer, res.Wind);
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
                foreach (var child in item.Input)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Source");
                    writer.WriteStartObject();
                    writer.WritePropertyName("Target");
                    writer.WriteStringValue(target);
                    writer.WritePropertyName("Id");
                    writer.WriteStringValue(child.SourceId);
                    writer.WriteEndObject();
                    writer.WritePropertyName("Weight");
                    writer.WriteNumberValue(child.Weight);
                    writer.WritePropertyName("Type");
                    writer.WriteStringValue(Enum.GetName(child.SourceComponent));
                    writer.WritePropertyName("Reflect");
                    writer.WriteBooleanValue(child.IsInverted);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
                writer.WritePropertyName("Output");
                writer.WriteStartArray();
                foreach (var child in item.Output)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Destination");
                    writer.WriteStartObject();
                    writer.WritePropertyName("Target");
                    writer.WriteStringValue(target);
                    writer.WritePropertyName("Id");
                    writer.WriteStringValue(child.DestinationId);
                    writer.WriteEndObject();
                    writer.WritePropertyName("VertexIndex");
                    writer.WriteNumberValue(child.ParticleIndex);
                    writer.WritePropertyName("Scale");
                    writer.WriteNumberValue(child.AngleScale);
                    writer.WritePropertyName("Weight");
                    writer.WriteNumberValue(child.Weight);
                    writer.WritePropertyName("Type");
                    writer.WriteStringValue(Enum.GetName(child.SourceComponent));
                    writer.WritePropertyName("Reflect");
                    writer.WriteBooleanValue(child.IsInverted);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
                writer.WritePropertyName("Vertices");
                writer.WriteStartArray();
                foreach (var child in item.Particles)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Position");
                    JsonExporter.Serialize(writer, child.InitialPosition);
                    writer.WritePropertyName("Mobility");
                    writer.WriteNumberValue(child.Mobility);
                    writer.WritePropertyName("Delay");
                    writer.WriteNumberValue(child.Delay);
                    writer.WritePropertyName("Acceleration");
                    writer.WriteNumberValue(child.Acceleration);
                    writer.WritePropertyName("Radius");
                    writer.WriteNumberValue(child.Radius);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
                writer.WritePropertyName("Normalization");
                JsonExporter.Serialize(writer, item.Normalization);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public void Dispose()
        {
        }

        private string GetDisplayName(IPPtr<MonoBehaviour> ptr)
        {
            var dict = BehaviorExporter.Deserialize(ptr, _assembly, _converter);
            if (dict == null)
            {
                return null;
            }

            var name = (string)dict["Name"];
            if (dict.Contains("DisplayName"))
            {
                var displayName = (string)dict["DisplayName"];
                name = displayName != "" ? displayName : name;
            }
            return name;
        }

        private IPPtr<T>? GetChildToPPtr<T>(IPPtr<MonoBehaviour> ptr)
            where T : Object
        {
            var data = BehaviorExporter.Deserialize(ptr, _assembly, _converter);
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
