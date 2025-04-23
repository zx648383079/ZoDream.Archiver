using System.Collections.Generic;
using System.IO;
using System.Numerics;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class PaperElementScanner(string package) : 
        IBundleElementScanner, IBundleStorage
    {
        public bool IsLoveAndDeepSpace => package.Contains("deepspace");

        public bool IsShiningNikki => package.Contains(".nn4");

        public Stream Open(string fullPath)
        {
            return File.OpenRead(fullPath);
        }

        public IBundleBinaryReader OpenRead(string fullPath)
        {
            return OpenRead(Open(fullPath), fullPath);
        }

        public IBundleBinaryReader OpenRead(Stream input, string fileName)
        {
            return new BundleBinaryReader(input, EndianType.BigEndian);
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
            var version = reader.Get<Version>();
            instance.StreamedClip = new StreamedClip(reader);
            instance.DenseClip = new();
            TryRead(reader, instance.DenseClip);
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                instance.ConstantClip = new ConstantClip(reader);
            }
            if (IsLoveAndDeepSpace)
            {
                instance.ACLClip = new LnDACLClip();
                instance.ACLClip.Read(reader);
            }
            if (version.LessThan(2018, 3)) //2018.3 down
            {
                instance.Binding = new ValueArrayConstant(reader);
            }
        }
        private void CreateInstance(IBundleBinaryReader reader, LayerConstant instance)
        {
            var version = reader.Get<Version>();

            instance.StateMachineIndex = reader.ReadUInt32();
            instance.StateMachineMotionSetIndex = reader.ReadUInt32();
            instance.BodyMask = new HumanPoseMask(reader);
            instance.SkeletonMask = new SkeletonMask(reader);
            if (IsLoveAndDeepSpace)
            {
                var m_GenericMask = new SkeletonMask(reader);
            }
            instance.Binding = reader.ReadUInt32();
            instance.LayerBlendingMode = reader.ReadInt32();
            if (version.GreaterThanOrEquals(4, 2)) //4.2 and up
            {
                instance.DefaultWeight = reader.ReadSingle();
            }
            instance.IKPass = reader.ReadBoolean();
            if (version.GreaterThanOrEquals(4, 2)) //4.2 and up
            {
                instance.SyncedLayerAffectsTiming = reader.ReadBoolean();
            }
            reader.AlignStream();
        }
        private void CreateInstance(IBundleBinaryReader reader, ClipMuscleConstant instance)
        {
            var version = reader.Get<Version>();
            if (IsLoveAndDeepSpace)
            {
                instance.StartX = reader.ReadXForm();
                if (version.GreaterThanOrEquals(5, 5))//5.5 and up
                {
                    instance.StopX = reader.ReadXForm();
                }
            }
            else
            {
                instance.DeltaPose = new HumanPose(reader);
                instance.StartX = reader.ReadXForm();
                if (version.GreaterThanOrEquals(5, 5))//5.5 and up
                {
                    instance.StopX = reader.ReadXForm();
                }
                instance.LeftFootStartX = reader.ReadXForm();
                instance.RightFootStartX = reader.ReadXForm();
                if (version.LessThan(5))//5.0 down
                {
                    instance.MotionStartX = reader.ReadXForm();
                    instance.MotionStopX = reader.ReadXForm();
                }
            }
            instance.AverageSpeed = version.GreaterThanOrEquals(5, 4) ? reader.ReadVector3Or4() : UnityReaderExtension.Parse(reader.ReadVector4());//5.4 and up
            instance.Clip = new Clip();
            TryRead(reader, instance.Clip);
            instance.StartTime = reader.ReadSingle();
            instance.StopTime = reader.ReadSingle();
            instance.OrientationOffsetY = reader.ReadSingle();
            instance.Level = reader.ReadSingle();
            instance.CycleOffset = reader.ReadSingle();
            instance.AverageAngularSpeed = reader.ReadSingle();
            instance.IndexArray = reader.ReadArray(r => r.ReadInt32());
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
            var version = reader.Get<Version>();
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
            var version = reader.Get<Version>();
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                if (IsLoveAndDeepSpace)
                {
                    reader.AlignStream();
                }

                int numChannels = reader.ReadInt32();
                instance.Channels = [];
                for (int i = 0; i < numChannels; i++)
                {
                    instance.Channels.Add(new MeshBlendShapeChannel(reader));
                }

                instance.FullWeights = reader.ReadArray(r => r.ReadSingle());
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
                                Index = index,
                                Vertex = (flags & 4) != 0 ? reader.ReadVector3Or4() : Vector3.Zero,
                                Normal = (flags & 2) != 0 ? reader.ReadVector3Or4() : Vector3.Zero,
                                Tangent = (flags & 1) != 0 ? reader.ReadVector3Or4() : Vector3.Zero,
                            };
                            instance.Vertices.Add(blendShapeVertex);
                        }
                        reader.AlignStream();

                        var stride = (uint)(varintVerticesSize / instance.Vertices.Length);
                        foreach (var shape in instance.Shapes)
                        {
                            shape.FirstVertex /= stride;
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
            instance.Lighting = reader.ReadBoolean();
            reader.AlignStream();
        }
        private void CreateInstance(IBundleBinaryReader reader, 
            SerializedSubProgram instance)
        {
            var version = reader.Get<Version>();
            if (IsLoveAndDeepSpace)
            {
                var m_CodeHash = new Hash128(reader);
            }

            instance.BlobIndex = reader.ReadUInt32();
            if (SerializedSubProgram.HasIsAdditionalBlob(reader.Get<SerializedType>()))
            {
                var m_IsAdditionalBlob = reader.ReadBoolean();
                reader.AlignStream();
            }
            instance.Channels = new ParserBindChannels(reader);

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
                instance.KeywordIndices = reader.ReadArray(r => r.ReadUInt16());
                if (version.GreaterThanOrEquals(2017)) //2017 and up
                {
                    reader.AlignStream();
                }
            }

            instance.ShaderHardwareTier = reader.ReadSByte();
            instance.GpuProgramType = (ShaderGpuProgramType)reader.ReadSByte();
            reader.AlignStream();

      
            instance.ReadBase(reader);
        }
        private void CreateInstance(IBundleBinaryReader reader, Material instance)
        {
            instance.ReadBase(reader);
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

            instance.SavedProperties = new PropertySheet(reader);
        }
    }
}
