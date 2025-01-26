using SkiaSharp;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using ZoDream.SpineExporter.Models;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.SpineExporter
{
    /// <summary>
    /// skel 文件读取
    /// </summary>
    public partial class SkeletonReader
    {
        public const int BONE_ROTATE = 0;
        public const int BONE_TRANSLATE = 1;
        public const int BONE_TRANSLATEX = 2;
        public const int BONE_TRANSLATEY = 3;
        public const int BONE_SCALE = 4;
        public const int BONE_SCALEX = 5;
        public const int BONE_SCALEY = 6;
        public const int BONE_SHEAR = 7;
        public const int BONE_SHEARX = 8;
        public const int BONE_SHEARY = 9;
        public const int BONE_INHERIT = 10;

        public const int SLOT_ATTACHMENT = 0;
        public const int SLOT_RGBA = 1;
        public const int SLOT_RGB = 2;
        public const int SLOT_RGBA2 = 3;
        public const int SLOT_RGB2 = 4;
        public const int SLOT_ALPHA = 5;

        public const int ATTACHMENT_DEFORM = 0;
        public const int ATTACHMENT_SEQUENCE = 1;

        public const int PATH_POSITION = 0;
        public const int PATH_SPACING = 1;
        public const int PATH_MIX = 2;

        public const int PHYSICS_INERTIA = 0;
        public const int PHYSICS_STRENGTH = 1;
        public const int PHYSICS_DAMPING = 2;
        public const int PHYSICS_MASS = 4;
        public const int PHYSICS_WIND = 5;
        public const int PHYSICS_GRAVITY = 6;
        public const int PHYSICS_MIX = 7;
        public const int PHYSICS_RESET = 8;

        public const int CURVE_LINEAR = 0;
        public const int CURVE_STEPPED = 1;
        public const int CURVE_BEZIER = 2;

        private float Scale = 1;
        private string[] _cacheItems = [];
        private Version _version = new();

        public IEnumerable<SkeletonRoot>? Read(Stream input)
        {
            var res = new SkeletonRoot()
            {
                Skeleton = new()
            };
            var reader = new EndianReader(input, EndianType.BigEndian);
            ReadHeader(reader, res.Skeleton);
            _version = Version.Parse(res.Skeleton.Version);
            res.Skeleton.X = reader.ReadSingle();
            res.Skeleton.Y = reader.ReadSingle();
            res.Skeleton.Width = reader.ReadSingle();
            res.Skeleton.Height = reader.ReadSingle();
            if (_version.Major >= 4)
            {
                res.Skeleton.ReferenceScale = reader.ReadSingle() * Scale;
            }
            var nonessential = reader.ReadBoolean();
            if (nonessential)
            {
                res.Skeleton.Fps = reader.ReadSingle();
                res.Skeleton.ImagesPath = ReadString(reader);
                res.Skeleton.AudioPath = ReadString(reader);
            }
            // ? 
            _cacheItems = ReadArray(reader, _ => ReadString(reader));


            // Bones
            var boneMap = new Dictionary<int, int>();
            res.Bones = ReadArray(reader, i => {
                var name = ReadString(reader);
                if (i > 0)
                {
                    boneMap.Add(i, ReadInt(reader, true));
                }
                var bone = new Bone
                {
                    Name = name,
                    Rotate = reader.ReadSingle(),
                    X = reader.ReadSingle() * Scale,
                    Y = reader.ReadSingle() * Scale,
                    ScaleX = reader.ReadSingle(),
                    ScaleY = reader.ReadSingle(),
                    ShearX = reader.ReadSingle(),
                    ShearY = reader.ReadSingle(),
                    Length = reader.ReadSingle() * Scale,
                    TransformMode = (TransformMode)ReadInt(reader, true)
                };
                //skinRequired
                reader.ReadBoolean();
                if (nonessential)
                {
                    ReadColor(reader); // Skip bone color.
                    if (_version.Major >= 4)
                    {
                        ReadString(reader);
                        reader.ReadBoolean();
                    }
                }
                return bone;
            });
            foreach (var item in boneMap)
            {
                res.Bones[item.Key].Parent = res.Bones[item.Value].Name;
            }
            boneMap.Clear();

            // Slots.
            res.Slots = ReadArray(reader, _ => {
                var slot = new Slot
                {
                    Name = ReadString(reader),
                    Bone = res.Bones[ReadInt(reader, true)].Name
                };
                var color = ReadColor(reader);
                var darkColor = ReadColor(reader, false); // 0x00rrggbb
                slot.Attachment = ReadStringRef(reader);
                var blendMode = (BlendMode)ReadInt(reader, true);
                if (nonessential && _version.Major >= 4)
                {
                    reader.ReadBoolean();
                }
                return slot;
            });
            // IK constraints.
            res.IkConstraints = ReadArray(reader, _ => {
                var ik = new IkConstraint
                {
                    Name = ReadString(reader),
                    Order = ReadInt(reader, true),
                };
                if (_version.Major < 4)
                {
                    reader.ReadBoolean();
                }
                ik.Bones = ReadArray(reader, _ => {
                    return res.Bones[ReadInt(reader, true)].Name;
                });
                ik.Target = res.Bones[ReadInt(reader, true)].Name;
                if (_version.Major >= 4)
                {
                    var flags = reader.ReadByte();
                    ik.BendDirection = (flags & 2) != 0 ? 1 : -1;
                    if ((flags & 32) != 0)
                    {
                        ik.Mix = (flags & 64) != 0 ? reader.ReadSingle() : 1;
                    }
                    if ((flags & 128) != 0)
                    {
                        // ik.Softness = 
                        reader.ReadSingle();// * Scale;
                    }
                } else
                {
                    ik.Mix = reader.ReadSingle();
                    reader.ReadSingle();
                    ik.BendDirection = reader.ReadByte();
                    reader.ReadBoolean();
                    reader.ReadBoolean();
                    reader.ReadBoolean();
                }

                return ik;
            });
            // Transform constraints.
            res.TransformConstraints = ReadArray(reader, _ => {
                var tc = new TransformConstraint
                {
                    Name = ReadString(reader),
                    Order = ReadInt(reader, true),
                };
                if (_version.Major < 4)
                {
                    reader.ReadBoolean();
                }
                tc.Bones = ReadArray(reader, _ => {
                    return res.Bones[ReadInt(reader, true)].Name;
                });
                tc.Target = res.Bones[ReadInt(reader, true)].Name;
                if (_version.Major >= 4)
                {
                    var flags = reader.ReadByte();
                    tc.Local = (flags & 2) != 0;
                    tc.Relative = (flags & 4) != 0;
                    if ((flags & 8) != 0)
                    {
                        tc.OffsetRotation = reader.ReadSingle();
                    }
                    if ((flags & 16) != 0)
                    {
                        tc.OffsetX = reader.ReadSingle();
                    }
                    if ((flags & 32) != 0)
                    {
                        tc.OffsetY = reader.ReadSingle();
                    }
                    if ((flags & 64) != 0)
                    {
                        tc.OffsetScaleX = reader.ReadSingle();
                    }
                    if ((flags & 128) != 0)
                    {
                        tc.OffsetScaleY = reader.ReadSingle();
                    }
                    flags = reader.ReadByte();
                    if ((flags & 1) != 0)
                    {
                        tc.OffsetShearY = reader.ReadSingle();
                    }
                    if ((flags & 2) != 0)
                    {
                        tc.RotateMix = reader.ReadSingle();
                    }
                    if ((flags & 4) != 0)
                    {
                        tc.TranslateXMix = reader.ReadSingle();
                    }
                    if ((flags & 8) != 0)
                    {
                        tc.TranslateYMix = reader.ReadSingle();
                    }
                    if ((flags & 16) != 0)
                    {
                        tc.ScaleXMix = reader.ReadSingle();
                    }
                    if ((flags & 32) != 0)
                    {
                        tc.ScaleYMix = reader.ReadSingle();
                    }
                    if ((flags & 64) != 0)
                    {
                        tc.ShearYMix = reader.ReadSingle();
                    }
                } else
                {
                    tc.Local = reader.ReadBoolean();
                    tc.Relative = reader.ReadBoolean();
                    tc.OffsetRotation = reader.ReadSingle();
                    tc.OffsetX = reader.ReadSingle() * Scale;
                    tc.OffsetY = reader.ReadSingle() * Scale;
                    tc.OffsetScaleX = reader.ReadSingle();
                    tc.OffsetScaleY = reader.ReadSingle();
                    tc.OffsetShearY = reader.ReadSingle();
                    tc.RotateMix = reader.ReadSingle();
                    tc.TranslateXMix = reader.ReadSingle();
                    tc.ScaleXMix = reader.ReadSingle();
                    tc.ShearYMix = reader.ReadSingle();
                }
                
                return tc;
            });
            // Path constraints
            res.PathConstraints = ReadArray(reader, _ => {
                var pc = new PathConstraint
                {
                    Name = ReadString(reader),
                    Order = ReadInt(reader, true),
                    
                };
                // skinRequired
                reader.ReadBoolean();
                pc.Bones = ReadArray(reader, _ => {
                    return res.Bones[ReadInt(reader, true)].Name;
                });
                pc.Target = res.Bones[ReadInt(reader, true)].Name;
                if (_version.Major >= 4)
                {
                    var flags = reader.ReadByte();
                    pc.PositionMode = (PositionMode)(flags & 1);
                    pc.SpacingMode = (SpacingMode)((flags >> 1) & 3);
                    pc.RotateMode = (RotateMode)((flags >> 3) & 3);
                    if ((flags & 128) != 0)
                    {
                        pc.OffsetRotation = reader.ReadSingle();
                    }
                } else
                {
                    pc.PositionMode = (PositionMode)ReadInt(reader, true);
                    pc.SpacingMode = (SpacingMode)ReadInt(reader, true);
                    pc.RotateMode = (RotateMode)ReadInt(reader, true);
                    pc.OffsetRotation = reader.ReadSingle();
                }
                
                pc.Position = reader.ReadSingle();
                if (pc.PositionMode == PositionMode.Fixed)
                {
                    pc.Position *= Scale;
                }
                pc.Spacing = reader.ReadSingle();
                if (pc.SpacingMode == SpacingMode.Fixed || pc.SpacingMode == SpacingMode.Length)
                {
                    pc.Spacing *= Scale;
                }
                pc.RotateMix = reader.ReadSingle();
                pc.TranslateXMix = reader.ReadSingle();
                pc.TranslateYMix = reader.ReadSingle();
                return pc;
            });
            if (_version.Major >= 4)
            {
                res.PhysicsConstraints = ReadArray(reader, _ => {
                    var pc = new PhysicsConstraint()
                    {
                        Name = ReadString(reader),
                        Order = ReadInt(reader, true),
                        Bone = res.Bones[ReadInt(reader, true)].Name,
                    };
                    var flags = reader.ReadByte();
                    // skinRequired = (flags & 1) != 0
                    if ((flags & 2) != 0)
                    {
                        pc.X = reader.ReadSingle();
                    }
                    if ((flags & 4) != 0)
                    {
                        pc.Y = reader.ReadSingle();
                    }
                    if ((flags & 8) != 0)
                    {
                        pc.Rotate = reader.ReadSingle();
                    }
                    if ((flags & 16) != 0)
                    {
                        pc.ScaleX = reader.ReadSingle();
                    }
                    if ((flags & 32) != 0)
                    {
                        pc.ShearX = reader.ReadSingle();
                    }
                    pc.Limit = ((flags & 64) != 0 ? reader.ReadSingle() : 5000) * Scale;
                    pc.Step = 1f / reader.ReadByte();
                    pc.Inertia = reader.ReadSingle();
                    pc.Strength = reader.ReadSingle();
                    pc.Damping = reader.ReadSingle();
                    pc.MassInverse = (flags & 128) != 0 ? reader.ReadSingle() : 1;
                    pc.Wind = reader.ReadSingle();
                    pc.Gravity = reader.ReadSingle();
                    flags = reader.ReadByte();
                    pc.Mix = (flags & 128) != 0 ? reader.ReadSingle() : 1;
                    return pc;
                });

            }

            // Default skin.
            var defaultSkin = ReadSkin(reader, true, res, nonessential);
            // Skins.
            var skinItems = ReadArray(reader, _ => {
                return ReadSkin(reader, false, res, nonessential);
            });
            if (defaultSkin is null)
            {
                res.Skins = skinItems;
            } else
            {
                res.Skins = [defaultSkin, .. skinItems];
            }
            // Events.
            res.Events = ReadArray(reader, _ => {
                var e = new Event()
                {
                    Name = ReadString(reader),
                    Int  = ReadInt(reader, false),
                    Float = reader.ReadSingle(),
                    String = ReadString(reader),
                    AudioPath = ReadString(reader),
                };
                if (!string.IsNullOrEmpty(e.AudioPath))
                {
                    e.Volume = reader.ReadSingle();
                    e.Balance = reader.ReadSingle();
                }
                return e;
            });
            // Animations.
            res.Animations = ReadArray(reader, _ => {
                return ReadAnimation(reader, res);
            });
            return [res];
        }

        private Skin ReadSkin(BinaryReader reader, bool defaultSkin, 
            SkeletonRoot res, bool nonessential)
        {
            var skin = new Skin()
            {
                Name = defaultSkin ? "default" : ReadString(reader),
            };
            if (!defaultSkin)
            {
                if (nonessential)
                {
                    ReadColor(reader);
                }
                // bones
                ReadArray(reader, _ => ReadInt(reader, true));
                // ikConstraints
                ReadArray(reader, _ => ReadInt(reader, true));
                // transformConstraints
                ReadArray(reader, _ => ReadInt(reader, true));
                // pathConstraints
                ReadArray(reader, _ => ReadInt(reader, true));
                if (_version.Major >= 4)
                {
                    // physicsConstraints
                    ReadArray(reader, _ => ReadInt(reader, true));
                }
                
            }
            var slotCount = ReadInt(reader, true);
            if (slotCount <= 0 && defaultSkin)
            {
                return null;
            }
            ReadArray(slotCount, _ => {
                var slotIndex = ReadInt(reader, true);
                ReadArray(reader, _ => {
                    var name = ReadStringRef(reader);
                    var attachment = ReadAttachment(reader, res, skin, slotIndex, name, nonessential);
                    if (attachment is null)
                    {
                        return;
                    }
                    skin.Add(slotIndex, name, attachment);
                });
            });
            return skin;
        }

        private AttachmentBase ReadAttachment(BinaryReader reader, SkeletonRoot res, Skin skin, int slotIndex, string attachmentName, bool nonessential)
        {
            var flags = _version.Major >= 4 ? reader.ReadByte() : 255;
            var name = (flags & 8) != 0 ? ReadStringRef(reader) : attachmentName;
            var type = (AttachmentType)(_version.Major >= 4 ? (flags & 0x7) : reader.ReadByte());
            switch (type)
            {
                case AttachmentType.Region:
                    if (_version.Major >= 4)
                    {
                        return new RegionAttachment()
                        {
                            Name = name,
                            Path = (flags & 16) != 0 ? ReadStringRef(reader) : name,
                            Color = (flags & 32) != 0 ? ReadColor(reader) : SKColors.White,
                            Sequence = (flags & 64) != 0 ? ReadSequence(reader) : null,
                            Rotation = (flags & 128) != 0 ? reader.ReadSingle() : 0,
                            X = reader.ReadSingle(),
                            Y = reader.ReadSingle(),
                            ScaleX = reader.ReadSingle(),
                            ScaleY = reader.ReadSingle(),
                            Width = reader.ReadSingle(),
                            Height = reader.ReadSingle(),
                        };
                    }
                    return new RegionAttachment()
                    {
                        Name = name,
                        Path = ReadStringRef(reader),
                        Rotation = reader.ReadSingle(),
                        X = reader.ReadSingle(),
                        Y = reader.ReadSingle(),
                        ScaleX = reader.ReadSingle(),
                        ScaleY = reader.ReadSingle(),
                        Width = reader.ReadSingle(),
                        Height = reader.ReadSingle(),
                        Color = ReadColor(reader),
                    };
                case AttachmentType.BoundingBox:
                    var b = new BoundingBoxAttachment()
                    {
                        Name = name
                    };
                    ReadVertices(reader, b, (flags & 16) != 0);
                    if (nonessential)
                    {
                        ReadColor(reader);
                    }
                    return b;
                case AttachmentType.Mesh:
                    var m = new MeshAttachment
                    {
                        Name = name,
                    };
                    if (_version.Major >= 4)
                    {
                        m.Path = (flags & 16) != 0 ? ReadStringRef(reader) : name;
                        m.Color = (flags & 32) != 0 ? ReadColor(reader) : SKColors.White;
                        m.Sequence = (flags & 64) != 0 ? ReadSequence(reader) : null;
                        m.HullLength = ReadInt(reader, true);
                    } else
                    {
                        m.Path = ReadStringRef(reader);
                        m.Color = ReadColor(reader);
                    }
                    ReadVertices(reader, m, (flags & 128) != 0);
                    if (_version.Major < 4)
                    {
                        m.HullLength = ReadInt(reader, true);
                    }
                    if (nonessential)
                    {
                        m.Edges = ReadArray(reader, _ => (int)reader.ReadInt16());
                        m.Width = reader.ReadSingle();
                        m.Height = reader.ReadSingle();
                    }
                    return m;
                case AttachmentType.LinkedMesh:
                    var l = new MeshAttachment()
                    {
                        Name = name,
                    };
                    if (_version.Major >= 4)
                    {
                        l.Path = (flags & 16) != 0 ? ReadStringRef(reader) : name;
                        l.Color = (flags & 32) != 0 ? ReadColor(reader) : SKColors.White;
                        l.Sequence = (flags & 64) != 0 ? ReadSequence(reader) : null;
                        var skinIndex = ReadInt(reader, true);
                    } else
                    {
                        l.Path = ReadStringRef(reader);
                        l.Color = ReadColor(reader);
                        var skinName = ReadStringRef(reader);
                    }
                    var inheritTimelines = (flags & 128) != 0;
                    
                    var parent = ReadStringRef(reader);

                    l.InheritDeform = _version.Major >= 4 ? inheritTimelines : reader.ReadBoolean();
                    if (nonessential)
                    {
                        l.Width = reader.ReadSingle();
                        l.Height = reader.ReadSingle();
                    }
                    return l;
                case AttachmentType.Path:
                    var p = new PathAttachment()
                    {
                        Name = name,
                    };
                    if (_version.Major >= 4)
                    {
                        p.Closed = (flags & 16) != 0;
                        p.ConstantSpeed = (flags & 32) != 0;
                    } else
                    {
                        p.Closed = reader.ReadBoolean();
                        p.ConstantSpeed = reader.ReadBoolean();
                    }
                    ReadVertices(reader, p, (flags & 64) != 0);
                    if (nonessential)
                    {
                        ReadColor(reader);
                    }
                    return p;
                case AttachmentType.Point:
                    var t = new PointAttachment()
                    {
                        Rotation = reader.ReadSingle(),
                        X = reader.ReadSingle(),
                        Y = reader.ReadSingle(),
                    };
                    if (nonessential)
                    {
                        ReadColor(reader);
                    }
                    return t;
                case AttachmentType.Clipping:
                    var c = new ClippingAttachment()
                    {
                        Name = name,
                        EndSlot = res.Slots[ReadInt(reader, true)].Name,

                    };
                    ReadVertices(reader, c, (flags & 16) != 0);
                    if (nonessential)
                    {
                        ReadColor(reader);
                    }
                    return c;
                default:
                    return null;
            }
        }

        private Animation ReadAnimation(BinaryReader reader, SkeletonRoot res)
        {
            var name = ReadString(reader);
            var timelines = new List<Timeline>();
            var duration = 0f;

            // Slot timelines.
            ReadArray(reader, _ => {
                var slotIndex = ReadInt(reader, true);
                ReadArray(reader, _ => {
                    var timelineType = reader.ReadByte();
                    var frameCount = ReadInt(reader, true);
                    switch (timelineType)
                    {
                        case SLOT_ATTACHMENT:
                            {
                                var timeline = new AttachmentTimeline(frameCount)
                                {
                                    SlotIndex = slotIndex,
                                };
                                ReadArray(frameCount, i => {
                                    timeline.Frames[i] = reader.ReadSingle();
                                    timeline.AttachmentNames[i] = ReadStringRef(reader);
                                });
                                timelines.Add(timeline);
                                duration = Math.Max(duration, timeline.Frames[frameCount - 1]);
                                break;
                            }
                        case SLOT_RGBA:
                            {
                                var timeline = new ColorTimeline(frameCount)
                                {
                                    SlotIndex = slotIndex
                                };
                                ReadArray(frameCount, i => {
                                    timeline.Frames[i] = reader.ReadSingle();
                                    timeline.ColorFrames[i] = ReadColor(reader);
                                    if (i < frameCount - 1)
                                    {
                                        ReadCurve(reader, i, timeline);
                                    }
                                });
                                timelines.Add(timeline);
                                duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);
                                break;
                            }
                        case SLOT_RGB:
                            {
                                var timeline = new RGBTimeline(frameCount)
                                {
                                    SlotIndex = slotIndex
                                };
                                ReadArray(frameCount, i => {
                                    timeline.Frames[i] = reader.ReadSingle();
                                    timeline.ColorFrames[i] = new SKColor(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                    if (i < frameCount - 1)
                                    {
                                        ReadCurve(reader, i, timeline);
                                    }
                                });
                                timelines.Add(timeline);
                                duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);
                                break;
                            }
                        case SLOT_ALPHA:
                            {
                                var timeline = new AlphaTimeline(frameCount)
                                {
                                    SlotIndex = slotIndex
                                };
                                ReadArray(frameCount, i => {
                                    timeline.Frames[i] = reader.ReadSingle();
                                    timeline.ColorFrames[i] = new SKColor(0, 0, 0, reader.ReadByte());
                                    if (i < frameCount - 1)
                                    {
                                        ReadCurve(reader, i, timeline);
                                    }
                                });
                                timelines.Add(timeline);
                                duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);
                                break;
                            }
                        case SLOT_RGBA2:
                            {
                                var timeline = new TwoColorTimeline(frameCount)
                                {
                                    SlotIndex = slotIndex,
                                };
                                ReadArray(frameCount, i => {
                                    timeline.Frames[i] = reader.ReadSingle();
                                    timeline.ColorFrames[i] = ReadColor(reader);
                                    timeline.Color2Frames[i] = new SKColor(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                    if (i < frameCount - 1)
                                    {
                                        ReadCurve(reader, i, timeline);
                                    }
                                });
                                timelines.Add(timeline);
                                duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);
                                break;
                            }
                        case SLOT_RGB2:
                            {
                                var timeline = new RGB2Timeline(frameCount)
                                {
                                    SlotIndex = slotIndex,
                                };
                                ReadArray(frameCount, i => {
                                    timeline.Frames[i] = reader.ReadSingle();
                                    timeline.ColorFrames[i] = new SKColor(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                    timeline.Color2Frames[i] = new SKColor(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                    if (i < frameCount - 1)
                                    {
                                        ReadCurve(reader, i, timeline);
                                    }
                                });
                                timelines.Add(timeline);
                                duration = Math.Max(duration, timeline.Frames[timeline.FrameCount - 1]);
                                break;
                            }
                    }
                });
            });

            // Bone timelines.
            ReadArray(reader, _ => {
                var boneIndex = ReadInt(reader, true);
                ReadArray(reader, _ => {
                    var timelineType = reader.ReadByte();
                    var frameCount = ReadInt(reader, true);
                    switch (timelineType)
                    {
                        case BONE_ROTATE:
                            {
                                var timeline = new RotateTimeline(frameCount)
                                {
                                    BoneIndex = boneIndex,
                                };
                                ReadArray(frameCount, i => {
                                    timeline.Frames[i] = reader.ReadSingle();
                                    timeline.AngleItems[i] = reader.ReadSingle();
                                    if (i < frameCount - 1)
                                    {
                                        ReadCurve(reader, i, timeline);
                                    }
                                });
                                timelines.Add(timeline);
                                duration = Math.Max(duration, timeline.Frames[frameCount - 1]);
                                break;
                            }
                        case BONE_TRANSLATE:
                        case BONE_SCALE:
                        case BONE_SHEAR:
                            {
                                TranslateTimeline timeline;
                                var timelineScale = 1f;
                                if (timelineType == BONE_SCALE)
                                    timeline = new ScaleTimeline(frameCount);
                                else if (timelineType == BONE_SHEAR)
                                    timeline = new ShearTimeline(frameCount);
                                else
                                {
                                    timeline = new TranslateTimeline(frameCount);
                                    timelineScale = Scale;
                                }
                                timeline.BoneIndex = boneIndex;
                                ReadArray(frameCount, i => {
                                    timeline.Frames[i] = reader.ReadSingle();
                                    timeline.Points[i] = new SKPoint(reader.ReadSingle() * timelineScale, reader.ReadSingle() * timelineScale);
                                  
                                    if (i < frameCount - 1)
                                    {
                                        ReadCurve(reader, i, timeline);
                                    }
                                });
                                timelines.Add(timeline);
                                duration = Math.Max(duration, timeline.Frames[frameCount - 1]);
                                break;
                            }
                    }
                });
            });

            // IK timelines.
            ReadArray(reader, _ => {
                var index = ReadInt(reader, true);
                var frameCount = ReadInt(reader, true);
                var timeline = new IkConstraintTimeline(frameCount)
                {
                    IkConstraintIndex = index,
                };
                ReadArray(frameCount, i => {
                    timeline.Frames[i] = reader.ReadSingle();
                    timeline.MixItems[i] = reader.ReadSingle();
                    timeline.DirectionItems[i] = reader.ReadByte();
                    if (i < frameCount - 1)
                    {
                        ReadCurve(reader, i, timeline);
                    }
                });
                timelines.Add(timeline);
                duration = Math.Max(duration, timeline.Frames[frameCount - 1]);
            });

            // Transform constraint timelines.
            ReadArray(reader, _ => {
                var index = ReadInt(reader, true);
                var frameCount = ReadInt(reader, true);
                var timeline = new TransformConstraintTimeline(frameCount)
                {
                    TransformConstraintIndex = index
                };
                ReadArray(frameCount, i => {
                    timeline.Frames[i] = reader.ReadSingle();
                    timeline.RotateItems[i] = reader.ReadSingle();
                    timeline.TranslateItems[i] = reader.ReadSingle();
                    timeline.ScaleItems[i] = reader.ReadSingle();
                    timeline.ShearItems[i] = reader.ReadSingle();
                    if (i < frameCount - 1)
                    {
                        ReadCurve(reader, i, timeline);
                    }
                });
                timelines.Add(timeline);
                duration = Math.Max(duration, timeline.Frames[frameCount - 1]);
            });

            // Path constraint timelines.
            ReadArray(reader, _ => {
                var index = ReadInt(reader, true);
                var data = res.PathConstraints[index];
                ReadArray(reader, _ => {
                    var timelineType = reader.ReadByte();
                    var frameCount = ReadInt(reader, true);
                    switch (timelineType)
                    {
                        case PATH_POSITION:
                        case PATH_SPACING:
                            {
                                PathConstraintPositionTimeline timeline;
                                float timelineScale = 1;
                                if (timelineType == PATH_SPACING)
                                {
                                    timeline = new PathConstraintSpacingTimeline(frameCount);
                                    if (data.SpacingMode == SpacingMode.Length || data.SpacingMode == SpacingMode.Fixed)
                                    {
                                        timelineScale = Scale;
                                    }
                                }
                                else
                                {
                                    timeline = new PathConstraintPositionTimeline(frameCount);
                                    if (data.PositionMode == PositionMode.Fixed)
                                    {
                                        timelineScale = Scale;
                                    }
                                }
                                timeline.PathConstraintIndex = index;
                                ReadArray(frameCount, i => {
                                    timeline.Frames[i] = reader.ReadSingle();
                                    timeline.PositionItems[i] = reader.ReadSingle() * timelineScale;
                                    if (i < frameCount - 1)
                                    {
                                        ReadCurve(reader, i, timeline);
                                    }
                                });
                                timelines.Add(timeline);
                                duration = Math.Max(duration, timeline.Frames[frameCount - 1]);
                                break;
                            }
                        case PATH_MIX:
                            {
                                var timeline = new PathConstraintMixTimeline(frameCount)
                                {
                                    PathConstraintIndex = index,
                                };
                                ReadArray(frameCount, i => {
                                    timeline.Frames[i] = reader.ReadSingle();
                                    timeline.RotateItems[i] = reader.ReadSingle();
                                    timeline.TranslateItems[i] = reader.ReadSingle();
                                    if (i < frameCount - 1)
                                    {
                                        ReadCurve(reader, i, timeline);
                                    }
                                });
                                timelines.Add(timeline);
                                duration = Math.Max(duration, timeline.Frames[frameCount - 1]);
                                break;
                            }
                    }
                });
            });

            // Deform timelines.
            ReadArray(reader, _ => {
                var skinIndex = ReadInt(reader, true);
                var skin = res.Skins[skinIndex];
                ReadArray(reader, _ => {
                    var slotIndex = ReadInt(reader, true);
                    ReadArray(reader, _ => {
                        var attachmentName = ReadString(reader);
                        skin.TryGet<VertexAttachment>(slotIndex, attachmentName, out var attachment);
                        var weighted = attachment!.Bones != null;
                        float[] vertices = attachment.Vertices;
                        var deformLength = weighted ? vertices.Length / 3 * 2 : vertices.Length;

                        var frameCount = ReadInt(reader, true);
                        var timeline = new DeformTimeline(frameCount)
                        {
                            SlotIndex = slotIndex,
                            Attachment = attachment,
                        };

                        ReadArray(frameCount, i => {
                            timeline.Frames[i] = reader.ReadSingle();
                            float[] deform;
                            int end = ReadInt(reader, true);
                            if (end == 0)
                                deform = weighted ? new float[deformLength] : vertices;
                            else
                            {
                                deform = new float[deformLength];
                                int start = ReadInt(reader, true);
                                end += start;
                                for (int v = start; v < end; v++)
                                {
                                    deform[v] = reader.ReadSingle() * Scale;
                                }
                                if (!weighted)
                                {
                                    for (int v = 0, vn = deform.Length; v < vn; v++)
                                    {
                                        deform[v] += vertices[v];
                                    }
                                }
                            }
                            timeline.Vertices[i] = deform;
                            if (i < frameCount - 1)
                            {
                                ReadCurve(reader, i, timeline);
                            }
                        });
                        timelines.Add(timeline);
                        duration = Math.Max(duration, timeline.Frames[frameCount - 1]);
                    });
                });
            });

            // Draw order timeline.
            int drawOrderCount = ReadInt(reader, true);
            if (drawOrderCount > 0)
            {
                var timeline = new DrawOrderTimeline(drawOrderCount);
                var slotCount = res.Slots.Length;
                ReadArray(drawOrderCount, i => {
                    timeline.Frames[i] = reader.ReadSingle();
                    int offsetCount = ReadInt(reader, true);
                    int[] drawOrder = new int[slotCount];
                    for (int ii = slotCount - 1; ii >= 0; ii--)
                    {
                        drawOrder[ii] = -1;
                    }
                    int[] unchanged = new int[slotCount - offsetCount];
                    int originalIndex = 0, unchangedIndex = 0;
                    for (int ii = 0; ii < offsetCount; ii++)
                    {
                        int slotIndex = ReadInt(reader, true);
                        // Collect unchanged items.
                        while (originalIndex != slotIndex)
                            unchanged[unchangedIndex++] = originalIndex++;
                        // Set changed items.
                        drawOrder[originalIndex + ReadInt(reader, true)] = originalIndex++;
                    }
                    // Collect remaining unchanged items.
                    while (originalIndex < slotCount)
                        unchanged[unchangedIndex++] = originalIndex++;
                    // Fill in unchanged items.
                    for (int ii = slotCount - 1; ii >= 0; ii--)
                        if (drawOrder[ii] == -1) drawOrder[ii] = unchanged[--unchangedIndex];
                    timeline.DrawOrders[i] = drawOrder;
                });
                timelines.Add(timeline);
                duration = Math.Max(duration, timeline.Frames[drawOrderCount - 1]);
            }

            // Event timeline.
            int eventCount = ReadInt(reader, true);
            if (eventCount > 0)
            {
                var timeline = new EventTimeline(eventCount);
                ReadArray(eventCount, i => {
                    timeline.Frames[i] = reader.ReadSingle();
                    var eventIndex = ReadInt(reader, true);
                    var eventData = res.Events[eventIndex];
                    
                    var e = new Event
                    {
                        Name = eventData.Name,
                        Int = ReadInt(reader, false),
                        Float = reader.ReadSingle(),
                        String = ReadString(reader) ?? eventData.String
                    };
                    if (eventData.AudioPath != null)
                    {
                        e.Volume = reader.ReadSingle();
                        e.Balance = reader.ReadSingle();
                    }
                    timeline.Events[i] = e;
                });
                timelines.Add(timeline);
                duration = Math.Max(duration, timeline.Frames[eventCount - 1]);
            }

            timelines.TrimExcess();
            return new Animation()
            {
                Name = name,
                Timelines = [.. timelines],
                Duration = duration,
            };
        }

        private void ReadCurve(BinaryReader reader, int frameIndex, CurveTimeline timeline)
        {
            switch (reader.ReadByte())
            {
                case CURVE_STEPPED:
                    timeline.SetStepped(frameIndex);
                    break;
                case CURVE_BEZIER:
                    timeline.SetCurve(frameIndex, 
                        reader.ReadSingle(), reader.ReadSingle(), 
                        reader.ReadSingle(), reader.ReadSingle());
                    break;
            }
        }

        private void ReadVertices(BinaryReader reader, VertexAttachment res, bool weighted)
        {
            var vertexCount = ReadInt(reader, true);
            res.WorldVerticesLength = vertexCount << 1;
            if (_version.Major < 4 && res is MeshAttachment m)
            {
                m.RegionUVs = ReadArray(res.WorldVerticesLength, _ => {
                    return reader.ReadSingle();
                });
                m.Triangles = ReadArray(reader, _ => {
                    return (int)reader.ReadInt16();
                });
            }
            if (_version.Major < 4)
            {
                weighted = reader.ReadBoolean();
            }
            if (!weighted)
            {
                res.Vertices = ReadArray(res.WorldVerticesLength, _ => {
                    return reader.ReadSingle() * Scale;
                });
            } else
            {
                var weights = new List<float>();
                var bones = new List<int>();
                ReadArray(vertexCount, _ => {
                    var boneCount = ReadInt(reader, true);
                    bones.Add(boneCount);
                    ReadArray(boneCount, _ => {
                        bones.Add(ReadInt(reader, true));
                        weights.Add(reader.ReadSingle() * Scale);
                        weights.Add(reader.ReadSingle() * Scale);
                        weights.Add(reader.ReadSingle() * Scale);
                    });
                });
                res.Vertices = [.. weights];
                res.Bones = [.. bones];
            }
            if (res is PathAttachment p)
            {
                p.Lengths = ReadArray(vertexCount / 3, _ => {
                    return reader.ReadSingle() * Scale;
                });
            }
            if (_version.Major >= 4 && res is MeshAttachment m2)
            {
                m2.RegionUVs = ReadArray(res.WorldVerticesLength, _ => {
                    return reader.ReadSingle();
                });
                m2.Triangles = ReadArray((res.WorldVerticesLength - m2.HullLength - 2) * 3, _ => {
                    return (int)reader.ReadInt16();
                });
            }
        }

        private Sequence ReadSequence(BinaryReader reader)
        {
            return new Sequence(ReadInt(reader, true))
            {
                Start = ReadInt(reader, true),
                Digits = ReadInt(reader, true),
                SetupIndex = ReadInt(reader, true),
            };
        }
        private void ReadHeader(BinaryReader reader, SkeletonHeader header) 
        {
            var pos = reader.BaseStream.Position;
            reader.BaseStream.Seek(8, SeekOrigin.Current);
            var stringByteCount = ReadInt(reader, true);
            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
            if (stringByteCount <= 13)
            {
                header.Hash = BinaryPrimitives.ReadInt64BigEndian(reader.ReadBytes(8)).ToString();
                header.Version = ReadString(reader);
                return;
            }
            header.Hash = ReadString(reader);
            header.Version = ReadString(reader);
        }

        private void ReadArray(BinaryReader reader, Action<int> cb)
        {
            ReadArray(ReadInt(reader, true), cb);
        }

        private void ReadArray(int length, Action<int> cb)
        {
            for (var i = 0; i < length; i++)
            {
                cb.Invoke(i);
            }
        }
        private T[] ReadArray<T>(BinaryReader reader, Func<int, T> cb)
        {
            return ReadArray(ReadInt(reader, true), cb);
        }

        private T[] ReadArray<T>(int length, Func<int, T> cb)
        {
            var items = new T[length];
            for (var i = 0; i < length; i++)
            {
                items[i] = cb.Invoke(i);
            }
            return items;
        }

        public int ReadInt(BinaryReader reader, bool optimizePositive)
        {
            int b = reader.ReadByte();
            int result = b & 0x7F;
            if ((b & 0x80) != 0)
            {
                b = reader.ReadByte();
                result |= (b & 0x7F) << 7;
                if ((b & 0x80) != 0)
                {
                    b = reader.ReadByte();
                    result |= (b & 0x7F) << 14;
                    if ((b & 0x80) != 0)
                    {
                        b = reader.ReadByte();
                        result |= (b & 0x7F) << 21;
                        if ((b & 0x80) != 0)
                        {
                            result |= (reader.ReadByte() & 0x7F) << 28;
                        }
                    }
                }
            }
            return optimizePositive ? result : ((result >> 1) ^ -(result & 1));
        }

        private SKColor ReadColor(BinaryReader reader, bool hasAlpha = true)
        {
            if (!hasAlpha)
            {
                _ = reader.ReadByte();
            }
            var r = reader.ReadByte();
            var g = reader.ReadByte();
            var b = reader.ReadByte();
            var a = hasAlpha ? reader.ReadByte() : byte.MaxValue;
            return new SKColor(r, g, b, a);
        }
        private string ReadStringRef(BinaryReader reader)
        {
            var index = ReadInt(reader, true);
            if (index < 1)
            {
                return string.Empty;
            }
            return _cacheItems[index - 1];
        }
        private string ReadString(BinaryReader reader)
        {
            var length = ReadInt(reader, true);
            if (length < 2)
            {
                return string.Empty;
            }
            return Encoding.UTF8.GetString(reader.ReadBytes(length - 1));
        }
    }
}
