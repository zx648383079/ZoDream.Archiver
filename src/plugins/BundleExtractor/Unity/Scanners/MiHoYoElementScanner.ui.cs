using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class MiHoYoElementScanner: BundleConverter
    {
        private readonly Type[] _includeItems = [

            ];
        public override bool CanConvert(Type objectType)
        {
            return _includeItems.Contains(objectType);
        }

        public override object? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            if (IsGI)
            {
                if (objectType ==  typeof(Texture2D))
                {
                    return ReadTexture2D(reader, serializer);
                }
               
            }
            if (objectType == typeof(AnimationClip))
            {
                return ReadAnimationClip(reader, serializer);
            }
            if (objectType == typeof(Animator))
            {
                return ReadAnimator(reader, serializer);
            }
            if (objectType == typeof(Clip))
            {
                return ReadClip(reader, serializer);
            }
            if (objectType == typeof(ClipMuscleConstant))
            {
                return ReadClipMuscleConstant(reader, serializer);
            }
            if (objectType == typeof(Mesh))
            {
                return ReadMesh(reader, serializer);
            }
            if (objectType == typeof(SerializedSubProgram))
            {
                return ReadSerializedSubProgram(reader, serializer);
            }
            if (objectType == typeof(Shader))
            {
                return ReadShader(reader, serializer);
            }
            if (objectType == typeof(SkinnedMeshRenderer))
            {
                return ReadSkinnedMeshRenderer(reader, serializer);
            }
            if (objectType == typeof(MiHoYoBinData))
            {
                return ReadMiHoYoBinData(reader, serializer);
            }
            //if (objectType == typeof(Renderer))
            //{
            //    return ReadRenderer(reader, serializer);
            //}
            return null;
        }

        private MiHoYoBinData ReadMiHoYoBinData(IBundleBinaryReader reader, 
            IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
            }
            var encrypteKey = (byte)0x0;
            var buffer = reader.ReadArray(r => r.ReadByte());
            if (encrypteKey > 0)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = (byte)(buffer[i] ^ encrypteKey);
                }
            }
            return new MiHoYoBinData()
            {
                Value = Encoding.UTF8.GetString(buffer)
            };
        }

        private MiHoYoACLClip ReadMiHoYoACLClip(IBundleBinaryReader reader,
            IBundleSerializer serializer)
        {
            var res = new MiHoYoACLClip();
            var byteCount = reader.ReadInt32();

            if (IsSRGroup)
            {
                byteCount *= 4;
            }

            res.m_ClipData = reader.ReadBytes(byteCount);
            reader.AlignStream();

            res.m_CurveCount = reader.ReadUInt32();

            if (IsSRGroup)
            {
                res.m_ConstCurveCount = reader.ReadUInt32();
            }
            return res;
        }
        private Clip ReadClip(IBundleBinaryReader reader,
            IBundleSerializer serializer)
        {
            var res = new Clip();
            var version = reader.Get<Version>();
            res.StreamedClip = serializer.Deserialize<StreamedClip>(reader);
            res.DenseClip = UnityConverter.ReadDenseClip(reader, serializer);
            if (IsSRGroup)
            {
                res.ACLClip = ReadMiHoYoACLClip(reader, serializer);
            }
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                res.ConstantClip = serializer.Deserialize<ConstantClip>(reader);
            }
            if (IsGIGroup || IsBH3Group || IsZZZCB1)
            {
                res.ACLClip = ReadMiHoYoACLClip(reader, serializer);
            }
            if (version.LessThan(2018, 3)) //2018.3 down
            {
                res.Binding = serializer.Deserialize<ValueArrayConstant>(reader);
            }
            return res;
        }

        private AnimationClip ReadAnimationClip(IBundleBinaryReader reader,
            IBundleSerializer serializer)
        {
            var res = new AnimationClip();
            var hasStreamingInfo = false;
            AnimationClipConverter.ReadBase(res, reader, serializer, () => {
                var version = reader.Get<Version>();
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
                    if (IsGI)
                    {
                        var muscleClipSize = reader.ReadInt32();
                        if (muscleClipSize < 0)
                        {
                            hasStreamingInfo = true;
                            res.MuscleClipSize = reader.ReadUInt32();
                            var pos = reader.Position;
                            res.MuscleClip = ReadGIClipMuscleConstant(reader, serializer);
                            reader.Position = pos + res.MuscleClipSize;
                        }
                        else if (muscleClipSize > 0)
                        {
                            res.MuscleClipSize = (uint)muscleClipSize;
                            res.MuscleClip = ReadClipMuscleConstant(reader, serializer);
                        }
                    }
                    else
                    {
                        res.MuscleClipSize = reader.ReadUInt32();
                        res.MuscleClip = ReadClipMuscleConstant(reader, serializer);
                    }
                }
                if (IsSRGroup)
                {
                    var m_AclClipData = reader.ReadArray(r => r.ReadByte());
                    var m_AclBindings = reader.ReadArray<GenericBinding>(serializer);
                    var m_AclRange = new KeyValuePair<float, float>(
                        reader.ReadSingle(), reader.ReadSingle());
                }
            });

            if (hasStreamingInfo)
            {
                res.StreamData = serializer.Deserialize<ResourceSource>(reader);
                if (!string.IsNullOrEmpty(res.StreamData.Source))
                {
                    var aclClip = res.MuscleClip.Clip.ACLClip as GIACLClip;

                    var fs = reader.Get<ISerializedFile>().OpenResource(res.StreamData);
                    using var ms = new MemoryStream();
                    ms.Write(aclClip.DatabaseData);

                    //ms.Write(res.GetData());
                    fs.CopyTo(ms);
                    //ms.AlignStream();

                    aclClip.DatabaseData = ms.ToArray();
                }
            }
            return res;
        }
        private Animator ReadAnimator(IBundleBinaryReader reader,
            IBundleSerializer serializer)
        {
            var res = new Animator();
            var version = reader.Get<Version>();
            AnimatorConverter.ReadBase(res, reader, serializer, () => {
                if (IsGISubGroup)
                {
                    var m_FBIKAvatar = serializer.Deserialize<PPtr>(reader); //FBIKAvatar placeholder
                }
                var m_CullingMode = reader.ReadInt32();

                if (version.GreaterThanOrEquals(4, 5)) //4.5 and up
                {
                    var m_UpdateMode = reader.ReadInt32();
                }

                if (IsSR)
                {
                    var m_MotionSkeletonMode = reader.ReadInt32();
                }
            });
            return res;
        }
        private SerializedSubProgram ReadSerializedSubProgram(IBundleBinaryReader reader,
            IBundleSerializer serializer)
        {
            var res = new SerializedSubProgram();
            var version = reader.Get<Version>();

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

            if (IsGI && (res.GpuProgramType == ShaderGpuProgramType.Unknown 
                || !Enum.IsDefined(typeof(ShaderGpuProgramType), res.GpuProgramType)))
            {
                reader.Position -= 4;
                var m_LocalKeywordIndices = reader.ReadArray(r => r.ReadUInt16());
                reader.AlignStream();

                res.ShaderHardwareTier = reader.ReadSByte();
                res.GpuProgramType = (ShaderGpuProgramType)reader.ReadSByte();
                reader.AlignStream();
            }
            SerializedSubProgramConverter.ReadBase(res, reader, serializer, () => { });
            return res;
        }
        private Mesh ReadMesh(IBundleBinaryReader reader,
            IBundleSerializer serializer)
        {
            var res = new Mesh();
            var version = reader.Get<Version>();
            var hasVertexColorSkinning = Convert.ToHexString(reader.Get<SerializedType>().OldTypeHash) is "413A501B79022BF2DF389A82002FC81F";
            MeshConverter.ReadBase(res, reader, serializer, () => {
                if (version.GreaterThanOrEquals(2, 6)) //2.6.0 and up
                {
                    if (version.GreaterThanOrEquals(2019)) //2019 and up
                    {
                        var m_BonesAABB = reader.ReadArray<MinMaxAABB>(serializer);
                        var m_VariableBoneCountWeights = reader.ReadArray(r => r.ReadUInt32());
                    }

                    var m_MeshCompression = reader.ReadByte();
                    if (version.GreaterThanOrEquals(4))
                    {
                        if (version.LessThan(5))
                        {
                            var m_StreamCompression = reader.ReadByte();
                        }
                        var m_IsReadable = reader.ReadBoolean();
                        if (IsBH3)
                        {
                            var m_IsHighPrecisionPosition = reader.ReadBoolean();
                            var m_IsHighPrecisionTangent = reader.ReadBoolean();
                            var m_IsHighPrecisionUv = reader.ReadBoolean();
                        }
                        var m_KeepVertices = reader.ReadBoolean();
                        var m_KeepIndices = reader.ReadBoolean();
                        if (IsBH3 && hasVertexColorSkinning)
                        {
                            var m_VertexColorSkinning = reader.ReadBoolean();
                        }
                    }
                    reader.AlignStream();
                    if (IsGISubGroup || (IsBH3 && hasVertexColorSkinning))
                    {
                        var m_PackSkinDataToUV2UV3 = reader.ReadBoolean();
                        reader.AlignStream();
                    }

                    //Unity fixed it in 2017.3.1p1 and later versions
                    if (version.GreaterThanOrEquals(2017, 3, 1, VersionType.Patch, 1) 
                    && m_MeshCompression == 0)//2017.3.xfx with no compression
                    {
                        var m_IndexFormat = reader.ReadInt32();
                        res.Use16BitIndices = m_IndexFormat == 0;
                    }

                    int m_IndexBuffer_size = reader.ReadInt32();
                    if (res.Use16BitIndices)
                    {
                        res.IndexBuffer = reader.ReadArray(m_IndexBuffer_size / 2, (r, _) => (uint)r.ReadUInt16());
                        reader.AlignStream();
                    }
                    else
                    {
                        res.IndexBuffer = reader.ReadArray(m_IndexBuffer_size / 4, (r, _) => r.ReadUInt32());
                    }
                }

                if (version.LessThan(3, 5)) //3.4.2 and earlier
                {
                    res.VertexCount = reader.ReadInt32();
                    res.Vertices = reader.ReadArray(res.VertexCount * 3, (r, _) => r.ReadSingle()); //Vector3

                    res.Skin = reader.ReadArray<BoneWeights4>(serializer);

                    res.BindPose = reader.ReadMatrixArray();

                    res.UV0 = reader.ReadArray(reader.ReadInt32() * 2, (r, _) => r.ReadSingle()); //Vector2

                    res.UV1 = reader.ReadArray(reader.ReadInt32() * 2, (r, _) => r.ReadSingle()); //Vector2

                    if (version.LessThanOrEquals(2, 5)) //2.5 and down
                    {
                        int m_TangentSpace_size = reader.ReadInt32();
                        res.Normals = new float[m_TangentSpace_size * 3];
                        res.Tangents = new float[m_TangentSpace_size * 4];
                        for (int v = 0; v < m_TangentSpace_size; v++)
                        {
                            res.Normals[v * 3] = reader.ReadSingle();
                            res.Normals[v * 3 + 1] = reader.ReadSingle();
                            res.Normals[v * 3 + 2] = reader.ReadSingle();
                            res.Tangents[v * 3] = reader.ReadSingle();
                            res.Tangents[v * 3 + 1] = reader.ReadSingle();
                            res.Tangents[v * 3 + 2] = reader.ReadSingle();
                            res.Tangents[v * 3 + 3] = reader.ReadSingle(); //handedness
                        }
                    }
                    else //2.6.0 and later
                    {
                        res.Tangents = reader.ReadArray(reader.ReadInt32() * 4, (r, _) => r.ReadSingle()); //Vector4

                        res.Normals = reader.ReadArray(reader.ReadInt32() * 3, (r, _) => r.ReadSingle()); //Vector3
                    }
                }
                else
                {
                    if (version.LessThan(2018, 2)) //2018.2 down
                    {
                        res.Skin = reader.ReadArray<BoneWeights4>(serializer);
                    }

                    if (version.LessThanOrEquals(4, 2)) //4.2 and down
                    {
                        res.BindPose = reader.ReadMatrixArray();
                    }

                    res.VertexData = serializer.Deserialize<VertexData>(reader);
                }

                if (version.GreaterThanOrEquals(2, 6) && !res.CollisionMeshBaked) //2.6.0 and later
                {
                    res.CompressedMesh = serializer.Deserialize<CompressedMesh>(reader);
                }

                reader.Position += 24; //AABB m_LocalAABB

                if (version.LessThanOrEquals(3, 4, 2)) //3.4.2 and earlier
                {
                    int m_Colors_size = reader.ReadInt32();
                    res.Colors = new float[m_Colors_size * 4];
                    for (int v = 0; v < m_Colors_size * 4; v++)
                    {
                        res.Colors[v] = (float)reader.ReadByte() / 0xFF;
                    }

                    int m_CollisionTriangles_size = reader.ReadInt32();
                    reader.Position += m_CollisionTriangles_size * 4; //UInt32 indices
                    int m_CollisionVertexCount = reader.ReadInt32();
                }


                int m_MeshUsageFlags = reader.ReadInt32();

                if (version.GreaterThanOrEquals(2022, 1)) //2022.1 and up
                {
                    int m_CookingOptions = reader.ReadInt32();
                }

                if (version.GreaterThanOrEquals(5)) //5.0 and up
                {
                    var m_BakedConvexCollisionMesh = reader.ReadArray(r => r.ReadByte());
                    reader.AlignStream();
                    var m_BakedTriangleCollisionMesh = reader.ReadArray(r => r.ReadByte());
                    reader.AlignStream();
                    if (IsBH3)
                    {
                        var m_MeshOptimized = reader.ReadBoolean();
                    }
                }

                if (IsZZZCB1)
                {
                    var m_CloseMeshDynamicCompression = reader.ReadBoolean();
                    reader.AlignStream();

                    var m_CompressLevelVertexData = reader.ReadInt32();
                    var m_CompressLevelNormalAndTangent = reader.ReadInt32();
                    var m_CompressLevelTexCoordinates = reader.ReadInt32();
                }

                if (IsGIGroup || version.GreaterThanOrEquals(2018, 2)) //2018.2 and up
                {
                    var m_MeshMetrics = new float[2];
                    m_MeshMetrics[0] = reader.ReadSingle();
                    m_MeshMetrics[1] = reader.ReadSingle();
                }

                if (IsGIGroup)
                {
                    var m_MetricsDirty = reader.ReadBoolean();
                    reader.AlignStream();
                    var m_CloseMeshDynamicCompression = reader.ReadBoolean();
                    reader.AlignStream();
                    if (!IsGICB1 && !IsGIPack)
                    {
                        var m_IsStreamingMesh = reader.ReadBoolean();
                        reader.AlignStream();
                    }
                }
            });
            return res;
        }
        private Shader ReadShader(IBundleBinaryReader reader,
            IBundleSerializer serializer)
        {
            var res = new Shader();
            ShaderConverter.ReadBase(res, reader, serializer, () => {
                if (IsGISubGroup)
                {
                    if (BinaryPrimitives.ReadInt32LittleEndian(res.CompressedBlob) == -1)
                    {
                        res.CompressedBlob = reader.ReadArray(r => r.ReadByte()); //blobDataBlocks
                        reader.AlignStream();
                    }
                }
            });
            return res;
        }
        private ClipMuscleConstant ReadClipMuscleConstant(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            if (IsGI)
            {
                return ReadGIClipMuscleConstant(reader, serializer);
            }
            var version = reader.Get<Version>();
            var res = new ClipMuscleConstant();
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
            res.AverageSpeed = version.GreaterThanOrEquals(5, 4) ? reader.ReadVector3Or4() : 
                reader.ReadVector4().AsVector3();//5.4 and up
            res.Clip = serializer.Deserialize<Clip>(reader);
            res.StartTime = reader.ReadSingle();
            res.StopTime = reader.ReadSingle();
            res.OrientationOffsetY = reader.ReadSingle();
            res.Level = reader.ReadSingle();
            res.CycleOffset = reader.ReadSingle();
            res.AverageAngularSpeed = reader.ReadSingle();

            var hasShortIndexArray = Convert.ToHexString(reader.Get<SerializedType>().OldTypeHash)
                is "E708B1872AE48FD688AC012DF4A7A178" or "055AA41C7639327940F8900103A10356";
            if (IsSR && hasShortIndexArray)
            {
                res.IndexArray = reader.ReadArray(r => (int)r.ReadInt16());
            }
            else
            {
                res.IndexArray = reader.ReadArray(r => r.ReadInt32());
            }
            ClipMuscleConstantConverter.ReadBase(res, reader, serializer, () => { });
            return res;
        }
        private SkinnedMeshRenderer ReadSkinnedMeshRenderer(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new SkinnedMeshRenderer();
            RendererConverter.Read(res, reader, serializer);
            if (IsGIGroup)
            {
                res.RootBone = reader.ReadPPtr<Transform>(serializer);
                res.AABB = serializer.Deserialize<Bounds>(reader);
                res.DirtyAABB = reader.ReadBoolean();
                reader.AlignStream();
            }
            return res;
        }
        private void ReadRenderer(Renderer res, IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            RendererConverter.ReadBase(res, reader, serializer);
            var isNewHeader = false;
            var version = reader.Get<Version>();
            if (version.Major < 5) //5.0 down
            {
                var m_Enabled = reader.ReadBoolean();
                var m_CastShadows = reader.ReadBoolean();
                var m_ReceiveShadows = reader.ReadBoolean();
                var m_LightmapIndex = reader.ReadByte();
            }
            else //5.0 and up
            {
                if (version.GreaterThanOrEquals(5, 4)) //5.4 and up
                {
                    if (IsGI)
                    {
                        isNewHeader = CheckHeader(reader, 0x1A);
                    }
                    if (IsBH3)
                    {
                        isNewHeader = CheckHeader(reader, 0x12);
                    }
                    var m_Enabled = reader.ReadBoolean();
                    var m_CastShadows = reader.ReadByte();
                    var m_ReceiveShadows = reader.ReadByte();
                    if (version.GreaterThanOrEquals(2017, 2)) //2017.2 and up
                    {
                        var m_DynamicOccludee = reader.ReadByte();
                    }
                    if (IsBH3Group)
                    {
                        var m_AllowHalfResolution = reader.ReadByte();
                        int m_EnableGpuQuery = isNewHeader ? reader.ReadByte() : 0;
                    }
                    if (IsGIGroup)
                    {
                        var m_ReceiveDecals = reader.ReadByte();
                        var m_EnableShadowCulling = reader.ReadByte();
                        var m_EnableGpuQuery = reader.ReadByte();
                        var m_AllowHalfResolution = reader.ReadByte();
                        if (!IsGICB1)
                        {
                            if (IsGI)
                            {
                                var m_AllowPerMaterialProp = isNewHeader ?
                                    reader.ReadByte() : 0;
                            }
                            var m_IsRainOccluder = reader.ReadByte();
                            if (!IsGICB2)
                            {
                                var m_IsDynamicAOOccluder = reader.ReadByte();
                                if (IsGI)
                                {
                                    var m_IsHQDynamicAOOccluder = reader.ReadByte();
                                    var m_IsCloudObject = reader.ReadByte();
                                    var m_IsInteriorVolume = reader.ReadByte();
                                }
                            }
                            if (!IsGIPack)
                            {
                                var m_IsDynamic = reader.ReadByte();
                            }
                            if (IsGI)
                            {
                                var m_UseTessellation = reader.ReadByte();
                                var m_IsTerrainTessInfo = isNewHeader ? reader.ReadByte() : 0;
                                var m_UseVertexLightInForward = isNewHeader ? reader.ReadByte() : 0;
                                var m_CombineSubMeshInGeoPass = isNewHeader ? reader.ReadByte() : 0;
                            }
                        }
                    }
                    if (version.Major >= 2021) //2021.1 and up
                    {
                        var m_StaticShadowCaster = reader.ReadByte();

                    }
                    var m_MotionVectors = reader.ReadByte();
                    var m_LightProbeUsage = reader.ReadByte();
                    var m_ReflectionProbeUsage = reader.ReadByte();
                    if (version.GreaterThanOrEquals(2019, 3)) //2019.3 and up
                    {
                        var m_RayTracingMode = reader.ReadByte();
                    }
                    if (version.Major >= 2020) //2020.1 and up
                    {
                        var m_RayTraceProcedural = reader.ReadByte();
                    }
                    if (IsGI || IsGICB3 || IsGICB3Pre)
                    {
                        var m_MeshShowQuality = reader.ReadByte();
                    }
                    reader.AlignStream();
                }
                else
                {
                    var m_Enabled = reader.ReadBoolean();
                    reader.AlignStream();
                    var m_CastShadows = reader.ReadByte();
                    var m_ReceiveShadows = reader.ReadBoolean();
                    reader.AlignStream();
                }

                if (version.Major >= 2018 || IsBH3 && isNewHeader) //2018 and up
                {
                    var m_RenderingLayerMask = reader.ReadUInt32();
                }

                if (version.GreaterThanOrEquals(2018, 3)) //2018.3 and up
                {
                    var m_RendererPriority = reader.ReadInt32();
                }

                var m_LightmapIndex = reader.ReadUInt16();
                var m_LightmapIndexDynamic = reader.ReadUInt16();
                if (IsGIGroup && (m_LightmapIndex != 0xFFFF || m_LightmapIndexDynamic != 0xFFFF))
                {
                    throw new Exception("Not Supported !! skipping....");
                }
            }

            if (version.Major >= 3) //3.0 and up
            {
                var m_LightmapTilingOffset = reader.ReadVector4();
            }

            if (version.Major >= 5) //5.0 and up
            {
                var m_LightmapTilingOffsetDynamic = reader.ReadVector4();
            }

            if (IsGIGroup)
            {
                var m_ViewDistanceRatio = reader.ReadSingle();
                var m_ShaderLODDistanceRatio = reader.ReadSingle();
            }
            res.Materials = reader.ReadPPtrArray<Material>(serializer);

            if (version.Major < 3) //3.0 down
            {
                var m_LightmapTilingOffset = reader.ReadVector4();
            }
            else //3.0 and up
            {
                if (version.GreaterThanOrEquals(5, 5)) //5.5 and up
                {
                    res.StaticBatchInfo = serializer.Deserialize<StaticBatchInfo>(reader);
                }
                else
                {
                    res.SubsetIndices = reader.ReadArray(r => r.ReadUInt32());
                }

                var m_StaticBatchRoot = reader.ReadPPtr<Transform>(serializer);
            }

            if (IsGIGroup)
            {
                var m_MatLayers = reader.ReadInt32();
            }

            var hasPrope = Convert.ToHexString(reader.Get<SerializedType>().OldTypeHash)
                is "F622BC5EE0E86D7BDF8C912DD94DCBF5" or "9255FA54269ADD294011FDA525B5FCAC";

            if (!IsSR || !hasPrope)
            {
                if (version.GreaterThanOrEquals(5, 4)) //5.4 and up
                {
                    var m_ProbeAnchor = reader.ReadPPtr<Transform>(serializer);
                    var m_LightProbeVolumeOverride = reader.ReadPPtr<GameObject>(serializer);
                }
                else if (version.GreaterThanOrEquals(3, 5)) //3.5 - 5.3
                {
                    var m_UseLightProbes = reader.ReadBoolean();
                    reader.AlignStream();

                    if (version.Major >= 5)//5.0 and up
                    {
                        var m_ReflectionProbeUsage = reader.ReadInt32();
                    }

                    var m_LightProbeAnchor = reader.ReadPPtr<Transform>(serializer); //5.0 and up m_ProbeAnchor
                }
            }

            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                if (version.Major == 4 && version.Minor == 3) //4.3
                {
                    var m_SortingLayer = reader.ReadInt16();
                }
                else
                {
                    var m_SortingLayerID = reader.ReadUInt32();
                }

                //SInt16 m_SortingLayer 5.6 and up
                var m_SortingOrder = reader.ReadInt16();
                reader.AlignStream();
                if (IsGIGroup || IsBH3)
                {
                    var m_UseHighestMip = reader.ReadBoolean();
                    reader.AlignStream();
                }
                if (IsSR)
                {
                    var RenderFlag = reader.ReadUInt32();
                    reader.AlignStream();
                }
            }
        }
        private Texture2D ReadTexture2D(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new Texture2D();
            UnityConverter.ReadTexture(res, reader, serializer);
            var version = reader.Get<Version>();
            var hash = Convert.ToHexString(reader.Get<SerializedType>().OldTypeHash);
            var hasGNFTexture = hash == "1D52BB98AA5F54C67C22C39E8B2E400F";
            var hasExternalMipRelativeOffset = hash is "1D52BB98AA5F54C67C22C39E8B2E400F" or "5390A985F58D5524F95DB240E8789704";
            res.Width = reader.ReadInt32();
            res.Height = reader.ReadInt32();
            var m_CompleteImageSize = reader.ReadInt32();
            if (version.GreaterThanOrEquals(2020)) //2020.1 and up
            {
                var m_MipsStripped = reader.ReadInt32();
            }
            res.TextureFormat = (TextureFormat)reader.ReadInt32();
            if (version.LessThan(5, 2)) //5.2 down
            {
                res.MipMap = reader.ReadBoolean();
            }
            else
            {
                res.MipCount = reader.ReadInt32();
            }
            if (version.GreaterThanOrEquals(2, 6)) //2.6.0 and up
            {
                var m_IsReadable = reader.ReadBoolean();
                if (hasGNFTexture)
                {
                    var m_IsGNFTexture = reader.ReadBoolean();
                }
            }
            if (version.GreaterThanOrEquals(2020, 1)) //2020.1 and up
            {
                var m_IsPreProcessed = reader.ReadBoolean();
            }
            if (version.GreaterThanOrEquals(2019, 3)) //2019.3 and up
            {
                var m_IgnoreMasterTextureLimit = reader.ReadBoolean();
            }
            if (version.GreaterThanOrEquals(2022, 2)) //2022.2 and up
            {
                reader.AlignStream(); //m_IgnoreMipmapLimit
                var m_MipmapLimitGroupName = reader.ReadAlignedString();
            }
            if (version.GreaterThanOrEquals(3)) //3.0.0 - 5.4
            {
                if (version.LessThanOrEquals(5, 4))
                {
                    var m_ReadAllowed = reader.ReadBoolean();
                }
            }
            if (version.GreaterThanOrEquals(2018, 2)) //2018.2 and up
            {
                var m_StreamingMipmaps = reader.ReadBoolean();
            }
            reader.AlignStream();
            if (hasGNFTexture)
            {
                var m_TextureGroup = reader.ReadInt32();
            }
            if (version.GreaterThanOrEquals(2018, 2)) //2018.2 and up
            {
                var m_StreamingMipmapsPriority = reader.ReadInt32();
            }
            var m_ImageCount = reader.ReadInt32();
            var m_TextureDimension = reader.ReadInt32();
            res.TextureSettings = serializer.Deserialize<GLTextureSettings>(reader);
            if (version.GreaterThanOrEquals(3)) //3.0 and up
            {
                var m_LightmapFormat = reader.ReadInt32();
            }
            if (version.GreaterThanOrEquals(3, 5)) //3.5.0 and up
            {
                var m_ColorSpace = reader.ReadInt32();
            }
            if (version.GreaterThanOrEquals(2020, 2)) //2020.2 and up
            {
                var m_PlatformBlob = reader.ReadArray(r => r.ReadByte());
                reader.AlignStream();
            }
            var imageDataSize = reader.ReadInt32();
            if (imageDataSize == 0 && version.GreaterThanOrEquals(5, 3))//5.3.0 and up
            {
                if (hasExternalMipRelativeOffset)
                {
                    var m_externalMipRelativeOffset = reader.ReadUInt32();
                }
                res.StreamData = serializer.Deserialize<ResourceSource>(reader);
            }

            if (!string.IsNullOrEmpty(res.StreamData.Source))
            {
                var container = reader.Get<ISerializedFile>();
                if (reader.TryGet<IDependencyBuilder>(out var builder))
                {
                    var fileName = container.FullPath;
                    var fileId = reader.Get<ObjectInfo>().FileID;
                    builder.AddDependencyEntry(fileName,
                        fileId,
                        res.StreamData.Source);
                }
                res.ImageData = container.OpenResource(res.StreamData);
            }
            else
            {
                res.ImageData = new PartialStream(reader.BaseStream, imageDataSize);
            }
            return res;
        }

    }
}
