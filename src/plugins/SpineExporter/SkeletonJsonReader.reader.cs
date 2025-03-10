using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Storage;
using ZoDream.SpineExporter.Models;

namespace ZoDream.SpineExporter
{
    public partial class SkeletonJsonReader
    {
        private float Scale = 1;

        public async Task<SkeletonRoot?> ReadAsync(IStorageFileEntry entry)
        {
            return Deserialize(await LocationStorage.ReadAsync(await entry.OpenReadAsync()), entry.FullPath);
        }

        public SkeletonRoot? Deserialize(string content, string fileName)
        {
            using var doc = JsonDocument.Parse(content);
            if (doc == null)
            {
                return null;
            }
            var root = doc.RootElement;
            var data = new SkeletonRoot();
            if (root.TryGetProperty("skeleton", out var element))
            {
                data.Skeleton = new SkeletonHeader()
                {
                    Hash = ReadString(element, "hash"),
                    Version = ReadString(element, "spine"),
                    Width = ReadSingle(element, "width"),
                    Height = ReadSingle(element, "height"),
                    Fps = ReadSingle(element, "fps"),
                    ImagesPath = ReadString(element, "images")
                };
            }
            data.Bones = ReadArray(root, "bones", (e, _) => {
                var bone = new Bone()
                {
                    Parent = ReadString(e, "parent"),
                    Name = ReadString(e, "name"),
                    Length = ReadSingle(e, "length") * Scale,
                    X = ReadSingle(e, "x") * Scale,
                    Y = ReadSingle(e, "y") * Scale,
                    Rotate = ReadSingle(e, "rotation"),
                    ScaleX = ReadSingle(e, "scaleX", 1),
                    ScaleY = ReadSingle(e, "scaleY", 1),
                    ShearX = ReadSingle(e, "shearX"),
                    ShearY = ReadSingle(e, "shearY"),
                    TransformMode = ReadEnum<TransformMode>(e, "transform")
                };
                return bone;
            });
            data.Slots = ReadArray(root, "slots", (e, _) => {
                var slot = new Slot()
                {
                    Name = ReadString(e, "name"),
                    Bone = ReadString(e, "bone"),
                    Color = ReadColor(e, "color"),
                    DarkColor = ReadColor(e, "dark"),
                    Attachment = ReadString(e, "attachment"),
                    BlendMode = ReadEnum<BlendMode>(e, "blend")
                };
                return slot;
            });
            data.IkConstraints = ReadArray(root, "ik", (e, _) => {
                var ik = new IkConstraint()
                {
                    Name = ReadString(e, "name"),
                    Order = ReadInt(e, "order"),
                    Bones = ReadArray(e, "bones", (i, _) => i.GetString()!),
                    Target = ReadString(e, "target"),
                    BendDirection = (byte)(ReadBoolean(e, "bendPositive", true) ? 1 : 0),
                    Mix = ReadSingle(e, "mix")
                };
                return ik;
            });
            data.TransformConstraints = ReadArray(root, "transform", (e, _) => {
                var tc = new TransformConstraint()
                {
                    Name = ReadString(e, "name"),
                    Order = ReadInt(e, "order"),
                    Bones = ReadArray(e, "bones", (i, _) => i.GetString()!),
                    Target = ReadString(e, "target"),
                    Local = ReadBoolean(e, "local"),
                    Relative = ReadBoolean(e, "relative"),
                    OffsetRotation = ReadSingle(e, "rotation"),
                    OffsetX = ReadSingle(e, "x") * Scale,
                    OffsetY = ReadSingle(e, "y") * Scale,
                    OffsetScaleX = ReadSingle(e, "scaleX"),
                    OffsetScaleY = ReadSingle(e, "scaleY"),
                    OffsetShearY = ReadSingle(e, "shearY"),
                    RotateMix = ReadSingle(e, "rotateMix", 1),
                    TranslateXMix = ReadSingle(e, "translateMix", 1),
                    ScaleXMix = ReadSingle(e, "scaleMix", 1),
                    ShearYMix = ReadSingle(e, "shearMix", 1)
                };
                return tc;
            });
            data.PathConstraints = ReadArray(root, "path", (e, _) => {
                var pc = new PathConstraint()
                {
                    Name = ReadString(e, "name"),
                    Order = ReadInt(e, "order"),
                    Bones = ReadArray(e, "bones", (i, _) => i.GetString()!),
                    Target = ReadString(e, "target"),
                    PositionMode = ReadEnum(e, "positionMode", PositionMode.Percent),
                    SpacingMode = ReadEnum(e, "spacingMode", SpacingMode.Length),
                    RotateMode = ReadEnum(e, "rotateMode", RotateMode.Tangent),
                    OffsetRotation = ReadSingle(e, "rotation"),
                    Position = ReadSingle(e, "position"),
                    Spacing = ReadSingle(e, "spacing"),
                    RotateMix = ReadSingle(e, "rotateMix", 1),
                    TranslateXMix = ReadSingle(e, "translateMix", 1)
                };
                if (pc.PositionMode == PositionMode.Fixed)
                {
                    pc.Position *= Scale;
                }
                if (pc.SpacingMode == SpacingMode.Length || pc.SpacingMode == SpacingMode.Fixed)
                {
                    pc.Spacing *= Scale;
                }
                return pc;
            });
            if (root.TryGetProperty("skins", out element))
            {
                if (element.ValueKind == JsonValueKind.Array)
                {
                    data.Skins = element.EnumerateArray().Select(item => {
                        var skin = new Skin()
                        {
                            Name = ReadString(item, "name"),
                        };
                        if (item.TryGetProperty("attachments", out var itemValue))
                        {
                            foreach (var item2 in itemValue.EnumerateObject())
                            {
                                var slotIndex = data.GetSlotIndex(item2.Name);
                                foreach (var it in item2.Value.EnumerateObject())
                                {
                                    var attachment = ReadAttachment(it.Value, skin, slotIndex, it.Name, data);
                                    if (attachment != null)
                                    {
                                        skin.Add(slotIndex, it.Name, attachment);
                                    }
                                }
                            }
                        }
                        return skin;
                    }).ToArray();
                } else
                {
                    data.Skins = element.EnumerateObject().Select(item => {
                        var skin = new Skin()
                        {
                            Name = item.Name,
                        };
                        foreach (var item2 in item.Value.EnumerateObject())
                        {
                            var slotIndex = data.GetSlotIndex(item2.Name);
                            foreach (var it in item2.Value.EnumerateObject())
                            {
                                var attachment = ReadAttachment(it.Value, skin, slotIndex, it.Name, data);
                                if (attachment != null)
                                {
                                    skin.Add(slotIndex, it.Name, attachment);
                                }
                            }
                        }
                        return skin;
                    }).ToArray();
                }
            } else
            {
                data.Skins = [];
            }
            data.Events = ReadObject(root, "events", (e, name) => {
                return new Event()
                {
                    Name = name,
                    Int = ReadInt(e, "int"),
                    Float = ReadSingle(e, "float"),
                    String = ReadString(e, "string"),
                };
            });
            data.Animations = ReadObject(root, "animations", (e, name) => {
                return ReadAnimation(e, name, data);
            });
            return data;
        }

        private Animation ReadAnimation(JsonElement element, string name, SkeletonRoot data)
        {
            var timelines = new List<Timeline>();
            var duration = 0f;

            // Slot timelines.
            ReadObject(element, "slots", (items, slotName) => {
                var slotIndex = data.GetSlotIndex(slotName);
                foreach (var item in items.EnumerateObject())
                {
                    var timelineName = item.Name;
                    var e = item.Value;
                    if (timelineName == "attachment")
                    {
                        var timeline = new AttachmentTimeline(e.GetArrayLength())
                        {
                            SlotIndex = slotIndex,
                        };
                        var frameIndex = 0;
                        foreach (var it in e.EnumerateArray())
                        {
                            timeline.Frames[frameIndex] = ReadSingle(it, "time");
                            timeline.AttachmentNames[frameIndex] = ReadString(it, "name");
                            frameIndex++;
                        }
                        timelines.Add(timeline);
                        duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);

                    }
                    else if (timelineName == "color")
                    {
                        var timeline = new ColorTimeline(e.GetArrayLength())
                        {
                            SlotIndex = slotIndex,
                        };
                        var frameIndex = 0;
                        foreach (var it in e.EnumerateArray())
                        {
                            timeline.Frames[frameIndex] = ReadSingle(it, "time");
                            timeline.ColorFrames[frameIndex] = ReadColor(it, "color")??SKColor.Empty;
                            ReadCurve(it, timeline, frameIndex);
                            frameIndex++;
                        }
                        timelines.Add(timeline);
                        duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);
                    }
                    else if (timelineName == "twoColor")
                    {
                        var timeline = new TwoColorTimeline(e.GetArrayLength())
                        {
                            SlotIndex = slotIndex,
                        };

                        var frameIndex = 0;
                        foreach (var it in e.EnumerateArray())
                        {
                            timeline.Frames[frameIndex] = ReadSingle(it, "time");
                            timeline.ColorFrames[frameIndex] = ReadColor(it, "light") ?? SKColor.Empty;
                            timeline.Color2Frames[frameIndex] = ReadColor(it, "dark") ?? SKColor.Empty;
                            ReadCurve(it, timeline, frameIndex);
                            frameIndex++;
                        }
                        timelines.Add(timeline);
                        duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);

                    }
                }
            });

            // Bone timelines.
            ReadObject(element, "bones", (items, boneName) => {
                var boneIndex = Array.FindIndex(data.Bones, i => i.Name == boneName);
                foreach (var item in items.EnumerateObject())
                {
                    var timelineName = item.Name;
                    var e = item.Value;
                    if (timelineName == "rotate")
                    {
                        var timeline = new RotateTimeline(e.GetArrayLength())
                        {
                            BoneIndex = boneIndex,
                        };

                        var frameIndex = 0;
                        foreach (var it in e.EnumerateArray())
                        {
                            timeline.Frames[frameIndex] = ReadSingle(it, "time");
                            timeline.AngleItems[frameIndex] = ReadSingle(it, "angle");
                            ReadCurve(it, timeline, frameIndex);
                            frameIndex++;
                        }
                        timelines.Add(timeline);
                        duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);

                    }
                    else if (timelineName == "translate" || timelineName == "scale" || timelineName == "shear")
                    {
                        TranslateTimeline timeline;
                        float timelineScale = 1;
                        if (timelineName == "scale")
                            timeline = new ScaleTimeline(e.GetArrayLength());
                        else if (timelineName == "shear")
                            timeline = new ShearTimeline(e.GetArrayLength());
                        else
                        {
                            timeline = new TranslateTimeline(e.GetArrayLength());
                            timelineScale = Scale;
                        }
                        timeline.BoneIndex = boneIndex;

                        var frameIndex = 0;
                        foreach (var it in e.EnumerateArray())
                        {
                            timeline.Frames[frameIndex] = ReadSingle(it, "time");

                            timeline.Points[frameIndex] = new SKPoint(ReadSingle(it, "x") * timelineScale, ReadSingle(it, "y") * timelineScale);
                            ReadCurve(it, timeline, frameIndex);
                            frameIndex++;
                        }
                        timelines.Add(timeline);
                        duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);

                    }

                }
            });

            // IK constraint timelines.
            ReadObject(element, "ik", (items, ikName) => {
                var ikIndex = Array.FindIndex(data.IkConstraints, i => i.Name == ikName);
                var timeline = new IkConstraintTimeline(items.GetArrayLength())
                {
                    IkConstraintIndex = ikIndex
                };
                var frameIndex = 0;
                foreach (var item in items.EnumerateArray())
                {
                    timeline.Frames[frameIndex] = ReadSingle(item, "time");
                    timeline.MixItems[frameIndex] = ReadSingle(item, "mix");
                    timeline.DirectionItems[frameIndex] = (byte)(ReadBoolean(item, "bendPositive", true) ? 1 : 0);
                    ReadCurve(item, timeline, frameIndex);
                    frameIndex++;
                }
                timelines.Add(timeline);
                duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);
                
            });

            // Transform constraint timelines.
            ReadObject(element, "transform", (items, tcName) => {
                var timeline = new TransformConstraintTimeline(items.GetArrayLength())
                {
                    TransformConstraintIndex = Array.FindIndex(data.TransformConstraints, i => i.Name == tcName)
                };
                var frameIndex = 0;
                foreach (var item in items.EnumerateArray())
                {
                    timeline.Frames[frameIndex] = ReadSingle(item, "time");
                    timeline.RotateItems[frameIndex] = ReadSingle(item, "rotateMix", 1);
                    timeline.TranslateItems[frameIndex] = ReadSingle(item, "translateMix", 1);
                    timeline.ScaleItems[frameIndex] = ReadSingle(item, "scaleMix", 1);
                    timeline.ShearItems[frameIndex] = ReadSingle(item, "shearMix", 1);
                    ReadCurve(item, timeline, frameIndex);
                    frameIndex++;
                }
                timelines.Add(timeline);
                duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);

            });

            // Path constraint timelines.
            ReadObject(element, "paths", (items, pcName) => {
                var pcIndex = Array.FindIndex(data.PathConstraints, i => i.Name == pcName);
                var pc = data.PathConstraints[pcIndex];
                foreach (var item in items.EnumerateObject())
                {
                    var timelineName = item.Name;
                    var e = item.Value;
                    if (timelineName == "position" || timelineName == "spacing")
                    {
                        PathConstraintPositionTimeline timeline;
                        float timelineScale = 1;
                        if (timelineName == "spacing")
                        {
                            timeline = new PathConstraintSpacingTimeline(e.GetArrayLength());
                            if (pc.SpacingMode == SpacingMode.Length || pc.SpacingMode == SpacingMode.Fixed)
                            { 
                                timelineScale = Scale;
                            }
                        }
                        else
                        {
                            timeline = new PathConstraintPositionTimeline(e.GetArrayLength());
                            if (pc.PositionMode == PositionMode.Fixed)
                            {
                                timelineScale = Scale;
                            }
                        }
                        timeline.PathConstraintIndex = pcIndex;
                        var frameIndex = 0;
                        foreach (var it in e.EnumerateArray())
                        {
                            timeline.Frames[frameIndex] = ReadSingle(it, "time");
                            timeline.PositionItems[frameIndex] = ReadSingle(it, timelineName) * timelineScale;
                            ReadCurve(it, timeline, frameIndex);
                            frameIndex++;
                        }
                        timelines.Add(timeline);
                        duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);
                    }
                    else if (timelineName == "mix")
                    {
                        var timeline = new PathConstraintMixTimeline(e.GetArrayLength())
                        {
                            PathConstraintIndex = pcIndex
                        };
                        var frameIndex = 0;
                        foreach (var it in e.EnumerateArray())
                        {
                            timeline.Frames[frameIndex] = ReadSingle(it, "time");
                            timeline.RotateItems[frameIndex] = ReadSingle(it, "rotateMix", 1);
                            timeline.TranslateItems[frameIndex] = ReadSingle(it, "translateMix", 1);
                            ReadCurve(it, timeline, frameIndex);
                            frameIndex++;
                        }
                        timelines.Add(timeline);
                        duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);
                    }
                }
            });

            // Deform timelines.
            ReadObject(element, "deform", (items, skinName) => {
                var skin = Array.Find(data.Skins, i => i.Name == skinName);
                foreach (var item in items.EnumerateObject())
                {
                    var slotIndex = Array.FindIndex(data.Slots, i => i.Name == item.Name);
                    foreach (var s in item.Value.EnumerateObject())
                    {
                        var e = s.Value;
                        skin!.TryGet<VertexAttachment>(slotIndex, s.Name, out var attachment);
                        bool weighted = attachment!.Bones != null;
                        float[] vertices = attachment.Vertices;
                        int deformLength = weighted ? vertices.Length / 3 * 2 : vertices.Length;

                        var timeline = new DeformTimeline(e.GetArrayLength())
                        {
                            SlotIndex = slotIndex,
                            Attachment = attachment
                        };
                        var frameIndex = 0;
                        foreach (var it in e.EnumerateArray())
                        {
                            float[] deform;
                            if (!it.TryGetProperty("vertices", out _))
                            {
                                deform = weighted ? new float[deformLength] : vertices;
                            }
                            else
                            {
                                deform = new float[deformLength];
                                var start = ReadInt(it, "offset");
                                float[] verticesValue = ReadArray(it, "vertices", (i,_) => i.GetSingle()! * Scale);
                                Array.Copy(verticesValue, 0, deform, start, Math.Min(verticesValue.Length, deformLength - start));
                                if (!weighted)
                                {
                                    for (int i = 0; i < deformLength; i++)
                                    {
                                        deform[i] += vertices[i];
                                    }
                                }
                            }
                            timeline.Frames[frameIndex] = ReadSingle(it, "time");
                            timeline.Vertices[frameIndex] = deform;
                            ReadCurve(it, timeline, frameIndex);
                            frameIndex++;
                        }
                        timelines.Add(timeline);
                        duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);
                    }
                }
            });

            // Draw order timeline.
            if (element.TryGetProperty("drawOrder", out var items) || element.TryGetProperty("draworder", out items))
            {
                var timeline = new DrawOrderTimeline(items.GetArrayLength());
                int slotCount = data.Slots.Length;
                var frameIndex = 0;
                foreach (var item in items.EnumerateArray())
                {
                    int[] drawOrder = null;
                    if (item.TryGetProperty("offsets", out var offsets))
                    {
                        drawOrder = new int[slotCount];
                        for (int i = slotCount - 1; i >= 0; i--)
                        {
                            drawOrder[i] = -1;
                        }
                        int[] unchanged = new int[slotCount - offsets.GetArrayLength()];
                        int originalIndex = 0, unchangedIndex = 0;
                        foreach (var it in offsets.EnumerateArray())
                        {
                            var slotName = ReadString(it, "slot");
                            int slotIndex = Array.FindIndex(data.Slots, i => i.Name == slotName);
                            // Collect unchanged items.
                            while (originalIndex != slotIndex)
                            {
                                unchanged[unchangedIndex++] = originalIndex++;
                            }
                            // Set changed items.
                            var index = originalIndex + (int)ReadSingle(it, "offset");
                            drawOrder[index] = originalIndex++;
                        }
                        // Collect remaining unchanged items.
                        while (originalIndex < slotCount)
                            unchanged[unchangedIndex++] = originalIndex++;
                        // Fill in unchanged items.
                        for (int i = slotCount - 1; i >= 0; i--)
                            if (drawOrder[i] == -1) drawOrder[i] = unchanged[--unchangedIndex];
                    }
                    timeline.Frames[frameIndex] = ReadSingle(item, "time");
                    timeline.DrawOrders[frameIndex] = drawOrder;
                    frameIndex++;
                }
                timelines.Add(timeline);
                duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);
            }

            // Event timeline.
            if (element.TryGetProperty("events", out var e))
            {
                var timeline = new EventTimeline(e.GetArrayLength());
                var frameIndex = 0;
                foreach (var item in e.EnumerateArray())
                {
                    var eventName = ReadString(item, "name");
                    timeline.Frames[frameIndex] = ReadSingle(item, "time");
                    timeline.Events[frameIndex] = new Event()
                    {
                        Name = eventName,
                        Int = ReadInt(item, "int"),
                        Float = ReadSingle(item, "float"),
                        String = ReadString(item, "string"),
                    };
                    frameIndex++;
                }
                timelines.Add(timeline);
                duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);
            }

            return new Animation
            {
                Name = name,
                Timelines = [..timelines],
                Duration = duration,
            };
        }

        private void ReadCurve(JsonElement element, CurveTimeline timeline, int frameIndex)
        {
            if (!element.TryGetProperty("curve", out var el))
            {
                return;
            }
            if (el.ValueKind == JsonValueKind.String && el.GetString() == "stepped")
            {
                timeline.SetStepped(frameIndex);
            } else
            {
                var items = el.ValueKind == JsonValueKind.Array ? 
                    el.EnumerateArray().Select(e => e.GetSingle()).ToArray() : [el.GetSingle()];
                if (items.Length >= 4)
                {
                    timeline.SetCurve(frameIndex, items[0], items[1], items[2], items[3]);
                }
            }
        }

        private AttachmentBase? ReadAttachment(JsonElement element, Skin skin, int slotIndex, string name, SkeletonRoot data)
        {
            name = ReadString(element, "name", name);

            var typeName = ReadString(element, "type", "region");
            if (typeName == "skinnedmesh") typeName = "weightedMesh";
            if (typeName == "weightedmesh") typeName = "mesh";
            if (typeName == "weightedlinkedmesh") typeName = "linkedMesh";
            var type = Enum.Parse<AttachmentType>(typeName, true);

            string path = ReadString(element, "path", name);

            switch (type)
            {
                case AttachmentType.Region:
                    var region = new RegionAttachment() 
                    {
                        Name = name,
                        Path = path,
                        X = ReadSingle(element, "x") * Scale,
                        Y = ReadSingle(element, "y") * Scale,
                        ScaleX = ReadSingle(element, "scaleX", 1),
                        ScaleY = ReadSingle(element, "scaleY", 1),
                        Rotation = ReadSingle(element, "rotation"),
                        Width = ReadSingle(element, "width", 32) * Scale,
                        Height = ReadSingle(element, "height", 32) * Scale,
                        Color = ReadColor(element, "color"),
                    };
                    return region;
                case AttachmentType.BoundingBox:
                    var box = new BoundingBoxAttachment()
                    {
                        Name = name
                    };
                    ReadVertices(element, box, ReadInt(element, "vertexCount") << 1);
                    return box;
                case AttachmentType.Mesh:
                case AttachmentType.LinkedMesh:
                    {
                        var mesh = new MeshAttachment()
                        {
                            Name = name,
                            Path = path,
                            Color = ReadColor(element, "color"),
                            Width = ReadSingle(element, "width") * Scale,
                            Height = ReadSingle(element, "height") * Scale,
                            
                        };
                        var parent = ReadString(element, "parent");
                        if (string.IsNullOrEmpty(parent))
                        {
                            mesh.InheritDeform = ReadBoolean(element, "deform", true);
                            //return mesh;
                        }

                        var uvs = ReadArray(element, "uvs", (e, _) => e.GetSingle()!);
                        ReadVertices(element, mesh, uvs.Length);
                        mesh.Triangles = ReadArray(element, "triangles", (e,_) => e.GetInt32());
                        mesh.RegionUVs = uvs;

                        mesh.HullLength = ReadInt(element, "hull") * 2;
                        mesh.Edges = ReadArray(element, "edges", (e, _) => e.GetInt32());
                        return mesh;
                    }
                case AttachmentType.Path:
                    {
                        var pathAttachment = new PathAttachment()
                        {
                            Name = name,
                            Closed = ReadBoolean(element, "closed"),
                            ConstantSpeed = ReadBoolean(element, "constantSpeed", true),
                        };
                        int vertexCount = ReadInt(element, "vertexCount");
                        ReadVertices(element, pathAttachment, vertexCount << 1);

                        // potential BOZO see Java impl
                        pathAttachment.Lengths = ReadArray(element, "lengths", (e, _) => e.GetSingle()! * Scale);
                        return pathAttachment;
                    }
                case AttachmentType.Point:
                    {
                        var point = new PointAttachment()
                        {
                            Name = name,
                            X = ReadSingle(element, "x") * Scale,
                            Y = ReadSingle(element, "y") * Scale,
                            Rotation = ReadSingle(element, "rotation"),
                            // Color = ReadColor(element, "color"),
                        };
                        return point;
                    }
                case AttachmentType.Clipping:
                    {
                        var clip = new ClippingAttachment
                        {
                            Name = name,
                            EndSlot = ReadString(element, "end")
                        };

                        ReadVertices(element, clip, ReadInt(element, "vertexCount") << 1);

                        // clip.Color = ReadColor(element, "color");
                        return clip;
                    }
            }
            return null;
        }

        private void ReadVertices(JsonElement element, VertexAttachment box, int verticesLength)
        {
            box.WorldVerticesLength = verticesLength;
            var vertices = ReadArray(element, "vertices", (e, _) => e.GetSingle()!);
            if (verticesLength == vertices.Length)
            {
                box.Vertices = vertices;
                return;
            }
            var weights = new List<float>();
            var bones = new List<int>();
            for (int i = 0, n = vertices.Length; i < n;)
            {
                int boneCount = (int)vertices[i++];
                bones.Add(boneCount);
                for (int nn = i + boneCount * 4; i < nn; i += 4)
                {
                    bones.Add((int)vertices[i]);
                    weights.Add(vertices[i + 1] * Scale);
                    weights.Add(vertices[i + 2] * Scale);
                    weights.Add(vertices[i + 3]);
                }
            }
            box.Bones = [.. bones];
            box.Vertices = [.. weights];
        }

        private static T[] ReadArray<T>(JsonElement element, string key, 
            Func<JsonElement, int, T> cb)
        {
            if (element.TryGetProperty(key, out var res))
            {
                var items = new List<T>();
                var i = 0;
                foreach (var item in res.EnumerateArray())
                {
                    items.Add(cb.Invoke(item, i++));
                }
                return [.. items];
            }
            return [];
        }
        private static void ReadArray(JsonElement element, string key,
            Action<JsonElement> cb)
        {
            if (element.TryGetProperty(key, out var res))
            {
                foreach (var item in res.EnumerateArray())
                {
                    cb.Invoke(item);
                }
            }
        }
        private static T[] ReadObject<T>(JsonElement element, string key,
            Func<JsonElement, string, T> cb)
        {
            if (element.TryGetProperty(key, out var res))
            {
                var items = new List<T>();
                foreach (var item in res.EnumerateObject())
                {
                    items.Add(cb.Invoke(item.Value, item.Name));
                }
                return [.. items];
            }
            return [];
        }

        private static void ReadObject(JsonElement element, string key,
            Action<JsonElement, string> cb)
        {
            if (element.TryGetProperty(key, out var res))
            {
                foreach (var item in res.EnumerateObject())
                {
                    cb.Invoke(item.Value, item.Name);
                }
            }
        }

        private static string ReadString(JsonElement element, string key, string def = "")
        {
            if (element.TryGetProperty(key, out var res))
            {
                return res.GetString() ?? def;
            }
            return def;
        }
        private static int ReadInt(JsonElement element, string key)
        {
            if (element.TryGetProperty(key, out var res))
            {
                return res.GetInt32();
            }
            return 0;
        }

        private static bool ReadBoolean(JsonElement element, string key, bool def = false)
        {
            if (element.TryGetProperty(key, out var res))
            {
                return res.GetBoolean();
            }
            return def;
        }
        private static float ReadSingle(JsonElement element, string key, float def = 0)
        {
            if (element.TryGetProperty(key, out var res))
            {
                return res.GetSingle();
            }
            return def;
        }

        private static SKColor? ReadColor(JsonElement element, string key)
        {
            if (element.TryGetProperty(key, out var res) && SKColor.TryParse(res.ToString(), out var color))
            {
                if (res.GetString()?.Length >= 8)
                {
                    return new SKColor(color.Alpha, color.Red, color.Green, color.Blue);    
                }
                return color;
            }
            return null;
        }

        private static T ReadEnum<T>(JsonElement element, string key, T def = default)
            where T : struct
        {
            if (element.TryGetProperty(key, out var res))
            {
                return Enum.Parse<T>(res.GetString(), true);
            }
            return def;
        }
    }
}
