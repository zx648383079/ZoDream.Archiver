using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class PaperElementScanner : BundleConverter 
    {

        private Type[] _includeItems = [
            typeof(BlendShapeData),
            typeof(Material),
            typeof(ClipMuscleConstant),
            typeof(Clip),
            typeof(LayerConstant),
            typeof(AnimationClip),
            typeof(MeshBlendShape),
            typeof(Shader),
            typeof(SerializedSubProgram),
            typeof(SerializedShaderState)
        ];

        public override bool CanConvert(Type objectType)
        {
            return _includeItems.Contains(objectType);
        }

        public override object? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            if (objectType == typeof(BlendShapeData))
            {
                return ReadBlendShapeData(reader, serializer);
            }
            if (objectType == typeof(Material))
            {
                return ReadMaterial(reader, serializer);
            }
            if (objectType == typeof(ClipMuscleConstant))
            {
                return ReadClipMuscleConstant(reader, serializer);
            }
            if (objectType == typeof(Clip))
            {
                return ReadClip(reader, serializer);
            }
            if (objectType == typeof(LayerConstant))
            {
                return ReadLayerConstant(reader, serializer);
            }
            if (objectType == typeof(AnimationClip))
            {
                return ReadAnimationClip(reader, serializer);
            }
            if (objectType == typeof(MeshBlendShape))
            {
                return ReadMeshBlendShape(reader, serializer);
            }
            if (objectType == typeof(Shader))
            {
                return ReadShader(reader, serializer);
            }
            if (objectType == typeof(SerializedSubProgram))
            {
                return ReadSerializedSubProgram(reader, serializer);
            }
            if (objectType == typeof(SerializedShaderState))
            {
                return ReadSerializedShaderState(reader, serializer);
            }
            return null;
        }

        private Clip ReadClip(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new Clip();
            var version = reader.Get<Version>();
            res.StreamedClip = serializer.Deserialize<StreamedClip>(reader);
            res.DenseClip = serializer.Deserialize<DenseClip>(reader);
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                res.ConstantClip = serializer.Deserialize<ConstantClip>(reader);
            }
            if (IsLoveAndDeepSpace)
            {
                res.ACLClip = serializer.Deserialize<LnDACLClip>(reader);
            }
            if (version.LessThan(2018, 3)) //2018.3 down
            {
                res.Binding = serializer.Deserialize<ValueArrayConstant>(reader);
            }
            return res;
        }
        private LayerConstant ReadLayerConstant(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new LayerConstant
            {
                StateMachineIndex = reader.ReadUInt32(),
                StateMachineMotionSetIndex = reader.ReadUInt32(),
                BodyMask = serializer.Deserialize<HumanPoseMask>(reader),
                SkeletonMask = serializer.Deserialize<SkeletonMask>(reader)
            };
            if (IsLoveAndDeepSpace)
            {
                var m_GenericMask = serializer.Deserialize<SkeletonMask>(reader);
            }
            res.Binding = reader.ReadUInt32();
            res.LayerBlendingMode = reader.ReadInt32();
            if (version.GreaterThanOrEquals(4, 2)) //4.2 and up
            {
                res.DefaultWeight = reader.ReadSingle();
            }
            res.IKPass = reader.ReadBoolean();
            if (version.GreaterThanOrEquals(4, 2)) //4.2 and up
            {
                res.SyncedLayerAffectsTiming = reader.ReadBoolean();
            }
            reader.AlignStream();
            return res;
        }
        private ClipMuscleConstant ReadClipMuscleConstant(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new ClipMuscleConstant();
            if (IsLoveAndDeepSpace)
            {
                res.StartX = reader.ReadXForm();
                if (version.GreaterThanOrEquals(5, 5))//5.5 and up
                {
                    res.StopX = reader.ReadXForm();
                }
            }
            else
            {
                res.DeltaPose = serializer.Deserialize<HumanPose>(reader);
                res.StartX = reader.ReadXForm();
                if (version.GreaterThanOrEquals(5, 5))//5.5 and up
                {
                    res.StopX = reader.ReadXForm();
                }
                res.LeftFootStartX = reader.ReadXForm();
                res.RightFootStartX = reader.ReadXForm();
                if (version.LessThan(5))//5.0 down
                {
                    res.MotionStartX = reader.ReadXForm();
                    res.MotionStopX = reader.ReadXForm();
                }
            }
            res.AverageSpeed = version.GreaterThanOrEquals(5, 4) ? reader.ReadVector3Or4() : reader.ReadVector4().AsVector3();//5.4 and up
            res.Clip = serializer.Deserialize<Clip>(reader);
            res.StartTime = reader.ReadSingle();
            res.StopTime = reader.ReadSingle();
            res.OrientationOffsetY = reader.ReadSingle();
            res.Level = reader.ReadSingle();
            res.CycleOffset = reader.ReadSingle();
            res.AverageAngularSpeed = reader.ReadSingle();
            res.IndexArray = reader.ReadArray(r => r.ReadInt32());
            ClipMuscleConstantConverter.ReadBase(res, reader, serializer, () => { });

            return res;
        }
        private Shader ReadShader(IBundleBinaryReader reader,
            IBundleSerializer serializer)
        {
            var res = new Shader();
            ShaderConverter.ReadBase(res, reader, serializer, () => {
                if (IsLoveAndDeepSpace)
                {
                    var codeOffsets = reader.Read2DArray((r, _, _) => r.ReadUInt32());
                    var codeCompressedLengths = reader.Read2DArray((r, _, _) => r.ReadUInt32());
                    var codeDecompressedLengths = reader.Read2DArray((r, _, _) => r.ReadUInt32());
                    var codeCompressedBlob = reader.ReadArray(r => r.ReadByte());
                    reader.AlignStream();
                }
            });
            return res;
        }
        private MeshBlendShape ReadMeshBlendShape(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new MeshBlendShape();
            MeshBlendShapeConverter.ReadBase(ref res, reader, serializer, () => { });
            var version = reader.Get<Version>();
            if (!IsLoveAndDeepSpace && version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                reader.AlignStream();
            }
            return res;
        }
        private AnimationClip ReadAnimationClip(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new AnimationClip();
            AnimationClipConverter.ReadBase(res, reader, serializer, () => {
                var version = reader.Get<Version>();
                if (IsLoveAndDeepSpace)
                {
                    reader.AlignStream();
                    var m_aclTransformCache = reader.ReadArray(r => r.ReadByte());
                    var m_aclScalarCache = reader.ReadArray(r => r.ReadByte());
                    var m_aclTransformTrackId2CurveId = reader.ReadArray<AclTransformTrackIDToBindingCurveID>(serializer);
            
                    var m_aclScalarTrackId2CurveId = reader.ReadArray(r => r.ReadUInt32());
                }
                res.Compressed = reader.ReadBoolean();
                if (version.GreaterThanOrEquals(4, 3))//4.3 and up
                {
                    res.UseHighQualityCurve = reader.ReadBoolean();
                }
                reader.AlignStream();
                res.RotationCurves = reader.ReadArray<QuaternionCurve>(serializer);

                res.CompressedRotationCurves = reader.ReadArray<CompressedAnimationCurve>(serializer);
     


                if (version.GreaterThanOrEquals(5, 3))//5.3 and up
                {
                    res.EulerCurves = reader.ReadArray<Vector3Curve>(serializer);
                }

                res.PositionCurves = reader.ReadArray<Vector3Curve>(serializer);

                res.ScaleCurves = reader.ReadArray<Vector3Curve>(serializer);

                res.FloatCurves = reader.ReadArray<FloatCurve>(serializer);

                if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
                {
                    res.PPtrCurves = reader.ReadArray<PPtrCurve>(serializer);
                }

                res.SampleRate = reader.ReadSingle();
                res.WrapMode = reader.ReadInt32();
                if (version.GreaterThanOrEquals(3, 4)) //3.4 and up
                {
                    res.Bounds = serializer.Deserialize<Bounds>(reader);
                }
                if (version.Major >= 4)//4.0 and up
                {
                    res.MuscleClipSize = reader.ReadUInt32();
                    res.MuscleClip = serializer.Deserialize<ClipMuscleConstant>(reader);
                   
                }
            });
            return res;
        }
        private BlendShapeData ReadBlendShapeData(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new BlendShapeData();
            BlendShapeDataConverter.ReadBase(res, reader, serializer, () => { });
            var version = reader.Get<Version>();
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                if (IsLoveAndDeepSpace)
                {
                    reader.AlignStream();
                }

                res.Channels = reader.ReadArray<MeshBlendShapeChannel>(serializer);

                res.FullWeights = reader.ReadArray(r => r.ReadSingle());
                if (IsLoveAndDeepSpace)
                {
                    var varintVerticesSize = reader.ReadInt32();
                    if (varintVerticesSize > 0)
                    {
                        var pos = reader.Position;
                        var items = new List<BlendShapeVertex>();
                        while (reader.Position < pos + varintVerticesSize)
                        {
                            var value = reader.ReadUInt32();
                            var index = value & 0x0FFFFFFF;
                            var flags = value >> 0x1D;
                            var blendShapeVertex = new BlendShapeVertex
                            {
                                Index = index,
                                Vertex = (flags & 4) != 0 ? reader.ReadVector3Or4() : Vector3.Zero,
                                Normal = (flags & 2) != 0 ? reader.ReadVector3Or4() : Vector3.Zero,
                                Tangent = (flags & 1) != 0 ? reader.ReadVector3Or4() : Vector3.Zero,
                            };
                            items.Add(blendShapeVertex);
                        }
                        res.Vertices = [.. items];
                        reader.AlignStream();

                        var stride = (uint)(varintVerticesSize / res.Vertices.Length);
                        for (int i = 0; i < res.Shapes.Length; i++)
                        {
                            // TODO 不确定
                            res.Shapes[i].FirstVertex /= stride;
                        }
                    }
                }
                if (IsShiningNikki && version.GreaterThanOrEquals(2019))
                {
                    var varintVertices = reader.ReadArray(r => r.ReadByte());
                }
            }
            return res;
        }
        private SerializedShaderState ReadSerializedShaderState(IBundleBinaryReader reader,
            IBundleSerializer serializer)
        {
            var res = new SerializedShaderState();
            SerializedShaderStateConverter.ReadBase(res, reader, serializer, () => { });
            if (IsLoveAndDeepSpace)
            {
                var m_OverrideKeywordAndStage = reader.ReadArray(_ => new KeyValuePair<string, uint>(reader.ReadAlignedString(), reader.ReadUInt32()));
            }
            res.Lighting = reader.ReadBoolean();
            reader.AlignStream();
            return res;
        }
        private SerializedSubProgram ReadSerializedSubProgram(IBundleBinaryReader reader,
            IBundleSerializer serializer)
        {
            var res = new SerializedSubProgram();
            var version = reader.Get<Version>();
            if (IsLoveAndDeepSpace)
            {
                var m_CodeHash = serializer.Deserialize<Hash128>(reader);
            }

            res.BlobIndex = reader.ReadUInt32();
            if (SerializedSubProgramConverter.HasIsAdditionalBlob(reader.Get<SerializedType>()))
            {
                var m_IsAdditionalBlob = reader.ReadBoolean();
                reader.AlignStream();
            }
            res.Channels = serializer.Deserialize<ParserBindChannels>(reader);

            if (version.GreaterThanOrEquals(2019) && version.LessThan(2021, 1) ||
                SerializedSubProgramConverter.HasGlobalLocalKeywordIndices(reader.Get<SerializedType>())) //2019 ~2021.1
            {
                var m_GlobalKeywordIndices = reader.ReadArray(r => r.ReadUInt16());
                reader.AlignStream();
                var m_LocalKeywordIndices = reader.ReadArray(r => r.ReadUInt16());
                reader.AlignStream();
            }
            else
            {
                res.KeywordIndices = reader.ReadArray(r => r.ReadUInt16());
                if (version.GreaterThanOrEquals(2017)) //2017 and up
                {
                    reader.AlignStream();
                }
            }

            res.ShaderHardwareTier = reader.ReadSByte();
            res.GpuProgramType = (ShaderGpuProgramType)reader.ReadSByte();
            reader.AlignStream();


            SerializedSubProgramConverter.ReadBase(res, reader, serializer, () => { });
            return res;
        }
        private Material ReadMaterial(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new Material();
            MaterialConverter.ReadBase(res, reader, serializer, () => { });
            var version = reader.Get<Version>();
            if (IsLoveAndDeepSpace || (IsShiningNikki && version.Major >= 2019))
            {
                var m_MaterialType = reader.ReadUInt32();
            }

            if (version.GreaterThanOrEquals(5, 1)) //5.1 and up
            {
                var stringTagMapSize = reader.ReadInt32();
                for (int i = 0; i < stringTagMapSize; i++)
                {
                    var first = reader.ReadAlignedString();
                    var second = reader.ReadAlignedString();
                }
            }


            if (version.GreaterThanOrEquals(5, 6)) //5.6 and up
            {
                var disabledShaderPasses = reader.ReadArray(r => r.ReadString());
            }

            res.SavedProperties = serializer.Deserialize<PropertySheet>(reader);
            return res;
        }
    }
}
