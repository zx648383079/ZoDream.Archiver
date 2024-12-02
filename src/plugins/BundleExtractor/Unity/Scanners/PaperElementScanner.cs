using System.Collections.Generic;
using System.IO;
using System.Numerics;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class PaperElementScanner(string package) : 
        IBundleElementScanner, IBundleStorage
    {
        public bool IsLoveAndDeepSpace => package.Contains("deepspace");

        public bool IsShiningNikki => package.Contains(".nn4");

        public Stream Open(string path)
        {
            return File.OpenRead(path);
        }

        public bool TryRead(IBundleBinaryReader reader, object instance)
        {
            if (instance is BlendShapeData b)
            {
                CreateInstance(reader, b);
                return true;
            }
            if (instance is Material m)
            {
                CreateInstance(reader, m);
                return true;
            }
            if (instance is ClipMuscleConstant c)
            {
                CreateInstance(reader, c);
                return true;
            }
            if (instance is Clip cl)
            {
                CreateInstance(reader, cl);
                return true;
            }
            if (instance is LayerConstant lc)
            {

                CreateInstance(reader, lc);
                return true;
            }
            if (instance is AnimationClip ac)
            {
                CreateInstance(reader, ac);
                return true;
            }
            if (instance is MeshBlendShape mb)
            {
                CreateInstance(reader, mb);
                return true;
            }
            if (instance is Shader s)
            {
                CreateInstance(reader, s);
                return true;
            }
            if (instance is SerializedSubProgram ss)
            {
                CreateInstance(reader, ss);
                return true;
            }
            if (instance is SerializedShaderState sss)
            {
                CreateInstance(reader, sss);
                return true;
            }
            if (instance is IElementLoader l)
            {
                l.Read(reader);
                return true;
            }
            return false;
        }

        private void CreateInstance(IBundleBinaryReader reader,
           Clip instance)
        {
            var version = reader.Get<UnityVersion>();
            instance.m_StreamedClip = new StreamedClip(reader);
            instance.m_DenseClip = new();
            TryRead(reader, instance.m_DenseClip);
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                instance.m_ConstantClip = new ConstantClip(reader);
            }
            if (IsLoveAndDeepSpace)
            {
                instance.m_ACLClip = new LnDACLClip();
                instance.m_ACLClip.Read(reader);
            }
            if (version.LessThan(2018, 3)) //2018.3 down
            {
                instance.m_Binding = new ValueArrayConstant(reader);
            }
        }
        private void CreateInstance(IBundleBinaryReader reader, LayerConstant instance)
        {
            var version = reader.Get<UnityVersion>();

            instance.m_StateMachineIndex = reader.ReadUInt32();
            instance.m_StateMachineMotionSetIndex = reader.ReadUInt32();
            instance.m_BodyMask = new HumanPoseMask(reader);
            instance.m_SkeletonMask = new SkeletonMask(reader);
            if (IsLoveAndDeepSpace)
            {
                var m_GenericMask = new SkeletonMask(reader);
            }
            instance.m_Binding = reader.ReadUInt32();
            instance.m_LayerBlendingMode = reader.ReadInt32();
            if (version.GreaterThanOrEquals(4, 2)) //4.2 and up
            {
                instance.m_DefaultWeight = reader.ReadSingle();
            }
            instance.m_IKPass = reader.ReadBoolean();
            if (version.GreaterThanOrEquals(4, 2)) //4.2 and up
            {
                instance.m_SyncedLayerAffectsTiming = reader.ReadBoolean();
            }
            reader.AlignStream();
        }
        private void CreateInstance(IBundleBinaryReader reader, ClipMuscleConstant instance)
        {
            var version = reader.Get<UnityVersion>();
            if (IsLoveAndDeepSpace)
            {
                instance.m_StartX = reader.ReadXForm();
                if (version.GreaterThanOrEquals(5, 5))//5.5 and up
                {
                    instance.m_StopX = reader.ReadXForm();
                }
            }
            else
            {
                instance.m_DeltaPose = new HumanPose(reader);
                instance.m_StartX = reader.ReadXForm();
                if (version.GreaterThanOrEquals(5, 5))//5.5 and up
                {
                    instance.m_StopX = reader.ReadXForm();
                }
                instance.m_LeftFootStartX = reader.ReadXForm();
                instance.m_RightFootStartX = reader.ReadXForm();
                if (version.LessThan(5))//5.0 down
                {
                    instance.m_MotionStartX = reader.ReadXForm();
                    instance.m_MotionStopX = reader.ReadXForm();
                }
            }
            instance.m_AverageSpeed = version.GreaterThanOrEquals(5, 4) ? reader.ReadVector3() : UnityReaderExtension.Parse(reader.ReadVector4());//5.4 and up
            instance.m_Clip = new Clip();
            TryRead(reader, instance.m_Clip);
            instance.m_StartTime = reader.ReadSingle();
            instance.m_StopTime = reader.ReadSingle();
            instance.m_OrientationOffsetY = reader.ReadSingle();
            instance.m_Level = reader.ReadSingle();
            instance.m_CycleOffset = reader.ReadSingle();
            instance.m_AverageAngularSpeed = reader.ReadSingle();
            instance.m_IndexArray = reader.ReadArray(r => r.ReadInt32());
            instance.ReadBase(reader);
        }
        private void CreateInstance(IBundleBinaryReader reader, 
            Shader instance)
        {
            instance.ReadBase(reader, () => {
                if (IsLoveAndDeepSpace)
                {
                    var codeOffsets = reader.Read2DArray((r, _, _) => r.ReadUInt32());
                    var codeCompressedLengths = reader.Read2DArray((r, _, _) => r.ReadUInt32());
                    var codeDecompressedLengths = reader.Read2DArray((r, _, _) => r.ReadUInt32());
                    var codeCompressedBlob = reader.ReadArray(r => r.ReadByte());
                    reader.AlignStream();
                }
            });
        }
        private void CreateInstance(IBundleBinaryReader reader, MeshBlendShape instance)
        {
            instance.ReadBase(reader);
            var version = reader.Get<UnityVersion>();
            if (!IsLoveAndDeepSpace && version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                reader.AlignStream();
            }
        }
        private void CreateInstance(IBundleBinaryReader reader, AnimationClip instance)
        {
            instance.ReadBase(reader, () => {
                var version = reader.Get<UnityVersion>();
                if (IsLoveAndDeepSpace)
                {
                    reader.AlignStream();
                    var m_aclTransformCache = reader.ReadArray(r => r.ReadByte());
                    var m_aclScalarCache = reader.ReadArray(r => r.ReadByte());
                    int numaclTransformTrackId2CurveId = reader.ReadInt32();
                    var m_aclTransformTrackId2CurveId = new List<AclTransformTrackIDToBindingCurveID>();
                    for (int i = 0; i < numaclTransformTrackId2CurveId; i++)
                    {
                        m_aclTransformTrackId2CurveId.Add(new AclTransformTrackIDToBindingCurveID(reader));
                    }
                    var m_aclScalarTrackId2CurveId = reader.ReadArray(r => r.ReadUInt32());
                }
                instance.m_Compressed = reader.ReadBoolean();
                if (version.GreaterThanOrEquals(4, 3))//4.3 and up
                {
                    instance.m_UseHighQualityCurve = reader.ReadBoolean();
                }
                reader.AlignStream();
                int numRCurves = reader.ReadInt32();
                instance.m_RotationCurves = [];
                for (int i = 0; i < numRCurves; i++)
                {
                    instance.m_RotationCurves.Add(new QuaternionCurve(reader));
                }

                int numCRCurves = reader.ReadInt32();
                instance.m_CompressedRotationCurves = [];
                for (int i = 0; i < numCRCurves; i++)
                {
                    instance.m_CompressedRotationCurves.Add(new CompressedAnimationCurve(reader));
                }


                if (version.GreaterThanOrEquals(5, 3))//5.3 and up
                {
                    int numEulerCurves = reader.ReadInt32();
                    instance.m_EulerCurves = [];
                    for (int i = 0; i < numEulerCurves; i++)
                    {
                        instance.m_EulerCurves.Add(new Vector3Curve(reader));
                    }
                }

                int numPCurves = reader.ReadInt32();
                instance.m_PositionCurves = [];
                for (int i = 0; i < numPCurves; i++)
                {
                    instance.m_PositionCurves.Add(new Vector3Curve(reader));
                }

                int numSCurves = reader.ReadInt32();
                instance.m_ScaleCurves = [];
                for (int i = 0; i < numSCurves; i++)
                {
                    instance.m_ScaleCurves.Add(new Vector3Curve(reader));
                }

                int numFCurves = reader.ReadInt32();
                instance.m_FloatCurves = [];
                for (int i = 0; i < numFCurves; i++)
                {
                    instance.m_FloatCurves.Add(new FloatCurve(reader));
                }

                if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
                {
                    int numPtrCurves = reader.ReadInt32();
                    instance.m_PPtrCurves = [];
                    for (int i = 0; i < numPtrCurves; i++)
                    {
                        instance.m_PPtrCurves.Add(new PPtrCurve(reader));
                    }
                }

                instance.m_SampleRate = reader.ReadSingle();
                instance.m_WrapMode = reader.ReadInt32();
                if (version.GreaterThanOrEquals(3, 4)) //3.4 and up
                {
                    instance.m_Bounds = new AABB(reader);
                }
                if (version.Major >= 4)//4.0 and up
                {
                    instance.m_MuscleClipSize = reader.ReadUInt32();
                    instance.m_MuscleClip = new();
                    TryRead(reader, instance.m_MuscleClip);
                   
                }
            });
        }
        private void CreateInstance(IBundleBinaryReader reader, BlendShapeData instance)
        {
            instance.ReadBase(reader);
            var version = reader.Get<UnityVersion>();
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                if (IsLoveAndDeepSpace)
                {
                    reader.AlignStream();
                }

                int numChannels = reader.ReadInt32();
                instance.channels = [];
                for (int i = 0; i < numChannels; i++)
                {
                    instance.channels.Add(new MeshBlendShapeChannel(reader));
                }

                instance.fullWeights = reader.ReadArray(r => r.ReadSingle());
                if (IsLoveAndDeepSpace)
                {
                    var varintVerticesSize = reader.ReadInt32();
                    if (varintVerticesSize > 0)
                    {
                        var pos = reader.Position;
                        while (reader.Position < pos + varintVerticesSize)
                        {
                            var value = reader.ReadUInt32();
                            var index = value & 0x0FFFFFFF;
                            var flags = value >> 0x1D;
                            var blendShapeVertex = new BlendShapeVertex
                            {
                                index = index,
                                vertex = (flags & 4) != 0 ? reader.ReadVector3() : Vector3.Zero,
                                normal = (flags & 2) != 0 ? reader.ReadVector3() : Vector3.Zero,
                                tangent = (flags & 1) != 0 ? reader.ReadVector3() : Vector3.Zero,
                            };
                            instance.vertices.Add(blendShapeVertex);
                        }
                        reader.AlignStream();

                        var stride = (uint)(varintVerticesSize / instance.vertices.Count);
                        foreach (var shape in instance.shapes)
                        {
                            shape.firstVertex /= stride;
                        }
                    }
                }
                if (IsShiningNikki && version.GreaterThanOrEquals(2019))
                {
                    var varintVertices = reader.ReadArray(r => r.ReadByte());
                }
            }
        }
        private void CreateInstance(IBundleBinaryReader reader, 
            SerializedShaderState instance)
        {
            instance.ReadBase(reader);
            if (IsLoveAndDeepSpace)
            {
                int numOverrideKeywordAndStage = reader.ReadInt32();
                var m_OverrideKeywordAndStage = new List<KeyValuePair<string, uint>>();
                for (int i = 0; i < numOverrideKeywordAndStage; i++)
                {
                    m_OverrideKeywordAndStage.Add(new KeyValuePair<string, uint>(reader.ReadAlignedString(), reader.ReadUInt32()));
                }
            }
            instance.lighting = reader.ReadBoolean();
            reader.AlignStream();
        }
        private void CreateInstance(IBundleBinaryReader reader, 
            SerializedSubProgram instance)
        {
            var version = reader.Get<UnityVersion>();
            if (IsLoveAndDeepSpace)
            {
                var m_CodeHash = new Hash128(reader);
            }

            instance.m_BlobIndex = reader.ReadUInt32();
            if (SerializedSubProgram.HasIsAdditionalBlob(reader.Get<SerializedType>()))
            {
                var m_IsAdditionalBlob = reader.ReadBoolean();
                reader.AlignStream();
            }
            instance.m_Channels = new ParserBindChannels(reader);

            if (version.GreaterThanOrEquals(2019) && version.LessThan(2021, 1) || 
                SerializedSubProgram.HasGlobalLocalKeywordIndices(reader.Get<SerializedType>())) //2019 ~2021.1
            {
                var m_GlobalKeywordIndices = reader.ReadArray(r => r.ReadUInt16());
                reader.AlignStream();
                var m_LocalKeywordIndices = reader.ReadArray(r => r.ReadUInt16());
                reader.AlignStream();
            }
            else
            {
                instance.m_KeywordIndices = reader.ReadArray(r => r.ReadUInt16());
                if (version.GreaterThanOrEquals(2017)) //2017 and up
                {
                    reader.AlignStream();
                }
            }

            instance.m_ShaderHardwareTier = reader.ReadSByte();
            instance.m_GpuProgramType = (ShaderGpuProgramType)reader.ReadSByte();
            reader.AlignStream();

      
            instance.ReadBase(reader);
        }
        private void CreateInstance(IBundleBinaryReader reader, Material instance)
        {
            instance.ReadBase(reader);
            var version = reader.Get<UnityVersion>();
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

            instance.m_SavedProperties = new UnityPropertySheet(reader);
        }
    }
}
