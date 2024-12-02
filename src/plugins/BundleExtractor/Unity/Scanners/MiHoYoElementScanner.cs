using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    public partial class MiHoYoElementScanner(string package) : 
        IBundleElementScanner, IBundleStorage
    {
        /// <summary>
        /// 原神
        /// </summary>
        public bool IsGI => package.Contains("genshin");

        internal bool IsGICB1 => IsGI;

        internal bool IsGIPack => IsGI;

        internal bool IsGICB2 => IsGI;

        internal bool IsGICB3 => IsGI;

        internal bool IsGICB3Pre => IsGI;

        internal bool IsGISubGroup => IsGI || IsGICB2 || IsGICB3 || IsGICB3Pre;

        internal bool IsGIGroup => IsGI || IsGIPack || IsGICB1 || IsGICB2 || IsGICB3
                || IsGICB3Pre;
        /// <summary>
        /// 铁道
        /// </summary>
        internal bool IsSR => package.Contains("hkrpg");

        internal bool IsSRCB2 => IsSR;

        internal bool IsSRGroup => IsSRCB2 || IsSR;
        /// <summary>
        /// 崩坏3
        /// </summary>
        internal bool IsBH3 => package.Contains("bh3");

        internal bool IsBH3Group => IsBH3;
        /// <summary>
        /// 绝区零
        /// </summary>
        internal bool IsZZZCB1 => package.Contains("zzz");

        public Stream Open(string path)
        {
            return File.OpenRead(path);
        }

        public bool TryRead(IBundleBinaryReader reader, object instance)
        {
            if (IsGI && instance is Texture2D t)
            {
                CreateInstance(reader, t);
                return true;
            }
            if (IsGI && instance is AnimationClip c)
            {
                CreateInstance(reader, c);
                return true;
            }
            if (instance is Animator a)
            {
                CreateInstance(reader, a);
                return true;
            }
            if (instance is Clip cl)
            {
                CreateInstance(reader, cl);
                return true;
            }
            if (instance is ClipMuscleConstant cm)
            {
                CreateInstance(reader, cm);
                return true;
            }
            if (instance is Mesh m)
            {
                CreateInstance(reader, m);
                return true;
            }
            if (instance is AnimationClip ac)
            {
                CreateInstance(reader, ac);
                return true;
            }
            if (instance is SerializedSubProgram ss)
            {
                CreateInstance(reader, ss);
                return true;
            }
            if (instance is Shader s)
            {
                CreateInstance(reader, s);
                return true;
            }
            if (instance is SkinnedMeshRenderer sm)
            {
                CreateInstance2(reader, sm);
                return true;
            }
            if (instance is UIRenderer r)
            {
                CreateInstance(reader, r);
                return true;
            }
            if (instance is IElementLoader l)
            {
                l.Read(reader);
                return true;
            }
            return false;
        }

        private MiHoYoACLClip CreateACLClip(IBundleBinaryReader reader)
        {
            var instance = new MiHoYoACLClip();
            CreateInstance2(reader, instance);
            return instance;
        }
        private void CreateInstance2(IBundleBinaryReader reader,
           MiHoYoACLClip instance)
        {
            var byteCount = reader.ReadInt32();

            if (IsSRGroup)
            {
                byteCount *= 4;
            }

            instance.m_ClipData = reader.ReadBytes(byteCount);
            reader.AlignStream();

            instance.m_CurveCount = reader.ReadUInt32();

            if (IsSRGroup)
            {
                instance.m_ConstCurveCount = reader.ReadUInt32();
            }
        }
        private void CreateInstance(IBundleBinaryReader reader,
           Clip instance)
        {
            var version = reader.Get<UnityVersion>();
            instance.m_StreamedClip = new StreamedClip(reader);
            instance.m_DenseClip = new();
            TryRead(reader, instance.m_DenseClip);
            if (IsSRGroup)
            {
                instance.m_ACLClip = CreateACLClip(reader);
            }
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                instance.m_ConstantClip = new ConstantClip(reader);
            }
            if (IsGIGroup || IsBH3Group || IsZZZCB1)
            {
                instance.m_ACLClip = CreateACLClip(reader);
            }
            if (version.LessThan(2018, 3)) //2018.3 down
            {
                instance.m_Binding = new ValueArrayConstant(reader);
            }
        }

        private void CreateInstance(IBundleBinaryReader reader,
            AnimationClip instance)
        {
            var hasStreamingInfo = false;
            instance.ReadBase(reader, () => {
                var version = reader.Get<UnityVersion>();
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
                    if (IsGI)
                    {
                        var muscleClipSize = reader.ReadInt32();
                        if (muscleClipSize < 0)
                        {
                            hasStreamingInfo = true;
                            instance.m_MuscleClipSize = reader.ReadUInt32();
                            var pos = reader.Position;
                            instance.m_MuscleClip = new();
                            GICreateInstance(reader, instance.m_MuscleClip);
                            reader.Position = pos + instance.m_MuscleClipSize;
                        }
                        else if (muscleClipSize > 0)
                        {
                            instance.m_MuscleClipSize = (uint)muscleClipSize;
                            instance.m_MuscleClip = new();
                            CreateInstance(reader, instance.m_MuscleClip);
                        }
                    }
                    else
                    {
                        instance.m_MuscleClipSize = reader.ReadUInt32();
                        instance.m_MuscleClip = new();
                        CreateInstance(reader, instance.m_MuscleClip);
                    }
                }
                if (IsSRGroup)
                {
                    var m_AclClipData = reader.ReadArray(r => r.ReadByte());
                    var aclBindingsCount = reader.ReadInt32();
                    var m_AclBindings = new List<GenericBinding>();
                    for (int i = 0; i < aclBindingsCount; i++)
                    {
                        m_AclBindings.Add(new GenericBinding(reader));
                    }
                    var m_AclRange = new KeyValuePair<float, float>(
                        reader.ReadSingle(), reader.ReadSingle());
                }
            });

            if (hasStreamingInfo)
            {
                instance.m_StreamData = new StreamingInfo(reader);
                if (!string.IsNullOrEmpty(instance.m_StreamData?.path))
                {
                    var aclClip = instance.m_MuscleClip.m_Clip.m_ACLClip as GIACLClip;

                    var res = ((UIReader)reader).OpenResource(instance.m_StreamData);
                    using var ms = new MemoryStream();
                    ms.Write(aclClip.m_DatabaseData);

                    //ms.Write(res.GetData());
                    res.CopyTo(ms);
                    //ms.AlignStream();

                    aclClip.m_DatabaseData = ms.ToArray();
                }
            }
        }
        private void CreateInstance(IBundleBinaryReader reader,
            Animator instance)
        {
            var version = reader.Get<UnityVersion>();
            instance.ReadBase(reader, () => {
                if (IsGISubGroup)
                {
                    var m_FBIKAvatar = new PPtr<UIObject>(reader); //FBIKAvatar placeholder
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
        }
        private void CreateInstance(IBundleBinaryReader reader,
            SerializedSubProgram instance)
        {
            var version = reader.Get<UnityVersion>();

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

            if (IsGI && (instance.m_GpuProgramType == ShaderGpuProgramType.Unknown 
                || !Enum.IsDefined(typeof(ShaderGpuProgramType), instance.m_GpuProgramType)))
            {
                reader.Position -= 4;
                var m_LocalKeywordIndices = reader.ReadArray(r => r.ReadUInt16());
                reader.AlignStream();

                instance.m_ShaderHardwareTier = reader.ReadSByte();
                instance.m_GpuProgramType = (ShaderGpuProgramType)reader.ReadSByte();
                reader.AlignStream();
            }
            instance.ReadBase(reader);
        }
        private void CreateInstance(IBundleBinaryReader reader,
            Mesh instance)
        {
            var version = reader.Get<UnityVersion>();
            var hasVertexColorSkinning = Convert.ToHexString(reader.Get<SerializedType>().OldTypeHash) is "413A501B79022BF2DF389A82002FC81F";
            instance.ReadBase(reader, () => {
                if (version.GreaterThanOrEquals(2, 6)) //2.6.0 and up
                {
                    if (version.GreaterThanOrEquals(2019)) //2019 and up
                    {
                        var m_BonesAABBSize = reader.ReadInt32();
                        var m_BonesAABB = new List<MinMaxAABB>();
                        for (int i = 0; i < m_BonesAABBSize; i++)
                        {
                            m_BonesAABB.Add(new MinMaxAABB(reader));
                        }

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
                    if (version.GreaterThanOrEquals(2017, 3, 1, UnityVersionType.Patch, 1) && m_MeshCompression == 0)//2017.3.xfx with no compression
                    {
                        var m_IndexFormat = reader.ReadInt32();
                        instance.m_Use16BitIndices = m_IndexFormat == 0;
                    }

                    int m_IndexBuffer_size = reader.ReadInt32();
                    if (instance.m_Use16BitIndices)
                    {
                        instance.m_IndexBuffer = new uint[m_IndexBuffer_size / 2];
                        for (int i = 0; i < m_IndexBuffer_size / 2; i++)
                        {
                            instance.m_IndexBuffer[i] = reader.ReadUInt16();
                        }
                        reader.AlignStream();
                    }
                    else
                    {
                        instance.m_IndexBuffer = reader.ReadArray(m_IndexBuffer_size / 4, (r, _) => r.ReadUInt32());
                    }
                }

                if (version.LessThan(3, 5)) //3.4.2 and earlier
                {
                    instance.m_VertexCount = reader.ReadInt32();
                    instance.m_Vertices = reader.ReadArray(instance.m_VertexCount * 3, (r, _) => r.ReadSingle()); //Vector3

                    var skinNum = reader.ReadInt32();
                    instance.m_Skin = [];
                    for (int s = 0; s < skinNum; s++)
                    {
                        instance.m_Skin.Add(new BoneWeights4(reader));
                    }

                    instance.m_BindPose = reader.ReadMatrixArray();

                    instance.m_UV0 = reader.ReadArray(reader.ReadInt32() * 2, (r, _) => r.ReadSingle()); //Vector2

                    instance.m_UV1 = reader.ReadArray(reader.ReadInt32() * 2, (r, _) => r.ReadSingle()); //Vector2

                    if (version.LessThanOrEquals(2, 5)) //2.5 and down
                    {
                        int m_TangentSpace_size = reader.ReadInt32();
                        instance.m_Normals = new float[m_TangentSpace_size * 3];
                        instance.m_Tangents = new float[m_TangentSpace_size * 4];
                        for (int v = 0; v < m_TangentSpace_size; v++)
                        {
                            instance.m_Normals[v * 3] = reader.ReadSingle();
                            instance.m_Normals[v * 3 + 1] = reader.ReadSingle();
                            instance.m_Normals[v * 3 + 2] = reader.ReadSingle();
                            instance.m_Tangents[v * 3] = reader.ReadSingle();
                            instance.m_Tangents[v * 3 + 1] = reader.ReadSingle();
                            instance.m_Tangents[v * 3 + 2] = reader.ReadSingle();
                            instance.m_Tangents[v * 3 + 3] = reader.ReadSingle(); //handedness
                        }
                    }
                    else //2.6.0 and later
                    {
                        instance.m_Tangents = reader.ReadArray(reader.ReadInt32() * 4, (r, _) => r.ReadSingle()); //Vector4

                        instance.m_Normals = reader.ReadArray(reader.ReadInt32() * 3, (r, _) => r.ReadSingle()); //Vector3
                    }
                }
                else
                {
                    if (version.LessThan(2018, 2)) //2018.2 down
                    {
                        var skinNum = reader.ReadInt32();
                        instance.m_Skin = new List<BoneWeights4>();
                        for (int s = 0; s < skinNum; s++)
                        {
                            instance.m_Skin.Add(new BoneWeights4(reader));
                        }
                    }

                    if (version.LessThanOrEquals(4, 2)) //4.2 and down
                    {
                        instance.m_BindPose = reader.ReadMatrixArray();
                    }

                    instance.m_VertexData = new VertexData(reader);
                }

                if (version.GreaterThanOrEquals(2, 6) && !instance.m_CollisionMeshBaked) //2.6.0 and later
                {
                    instance.m_CompressedMesh = new CompressedMesh(reader);
                }

                reader.Position += 24; //AABB m_LocalAABB

                if (version.LessThanOrEquals(3, 4, 2)) //3.4.2 and earlier
                {
                    int m_Colors_size = reader.ReadInt32();
                    instance.m_Colors = new float[m_Colors_size * 4];
                    for (int v = 0; v < m_Colors_size * 4; v++)
                    {
                        instance.m_Colors[v] = (float)reader.ReadByte() / 0xFF;
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
        }
        private void CreateInstance(IBundleBinaryReader reader,
            Shader instance)
        {
            instance.ReadBase(reader, () => {
                if (IsGISubGroup)
                {
                    if (BinaryPrimitives.ReadInt32LittleEndian(instance.compressedBlob) == -1)
                    {
                        instance.compressedBlob = reader.ReadArray(r => r.ReadByte()); //blobDataBlocks
                        reader.AlignStream();
                    }
                }
            });
        }
        private void CreateInstance(IBundleBinaryReader reader, ClipMuscleConstant instance)
        {
            if (IsGI)
            {
                GICreateInstance(reader, instance);
                return;
            }
            var version = reader.Get<UnityVersion>();
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
            instance.m_AverageSpeed = version.GreaterThanOrEquals(5, 4) ? reader.ReadVector3() : 
                UnityReaderExtension.Parse(reader.ReadVector4());//5.4 and up
            instance.m_Clip = new Clip();
            TryRead(reader, instance.m_Clip);
            instance.m_StartTime = reader.ReadSingle();
            instance.m_StopTime = reader.ReadSingle();
            instance.m_OrientationOffsetY = reader.ReadSingle();
            instance.m_Level = reader.ReadSingle();
            instance.m_CycleOffset = reader.ReadSingle();
            instance.m_AverageAngularSpeed = reader.ReadSingle();

            var hasShortIndexArray = Convert.ToHexString(reader.Get<SerializedType>().OldTypeHash)
                is "E708B1872AE48FD688AC012DF4A7A178" or "055AA41C7639327940F8900103A10356";
            if (IsSR && hasShortIndexArray)
            {
                instance.m_IndexArray = reader.ReadArray(r => (int)r.ReadInt16());
            }
            else
            {
                instance.m_IndexArray = reader.ReadArray(r => r.ReadInt32());
            }
            instance.ReadBase(reader);
        }
        private void CreateInstance2(IBundleBinaryReader reader, SkinnedMeshRenderer instance)
        {
            instance.ReadBase(reader);
            instance.ReadBase2(reader);
            if (IsGIGroup)
            {
                instance.m_RootBone = new PPtr<Transform>(reader);
                instance.m_AABB = new AABB(reader);
                instance.m_DirtyAABB = reader.ReadBoolean();
                reader.AlignStream();
            }
        }
        private void CreateInstance(IBundleBinaryReader reader, UIRenderer instance)
        {
            instance.ReadBase(reader);
            var isNewHeader = false;
            var version = reader.Get<UnityVersion>();
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
            var m_MaterialsSize = reader.ReadInt32();
            instance.m_Materials = [];
            for (int i = 0; i < m_MaterialsSize; i++)
            {
                instance.m_Materials.Add(new PPtr<Material>(reader));
            }

            if (version.Major < 3) //3.0 down
            {
                var m_LightmapTilingOffset = reader.ReadVector4();
            }
            else //3.0 and up
            {
                if (version.GreaterThanOrEquals(5, 5)) //5.5 and up
                {
                    instance.m_StaticBatchInfo = new StaticBatchInfo(reader);
                }
                else
                {
                    instance.m_SubsetIndices = reader.ReadArray(r => r.ReadUInt32());
                }

                var m_StaticBatchRoot = new PPtr<Transform>(reader);
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
                    var m_ProbeAnchor = new PPtr<Transform>(reader);
                    var m_LightProbeVolumeOverride = new PPtr<GameObject>(reader);
                }
                else if (version.GreaterThanOrEquals(3, 5)) //3.5 - 5.3
                {
                    var m_UseLightProbes = reader.ReadBoolean();
                    reader.AlignStream();

                    if (version.Major >= 5)//5.0 and up
                    {
                        var m_ReflectionProbeUsage = reader.ReadInt32();
                    }

                    var m_LightProbeAnchor = new PPtr<Transform>(reader); //5.0 and up m_ProbeAnchor
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
        private void CreateInstance(IBundleBinaryReader reader, Texture2D instance)
        {
            instance.ReadBase(reader);
            var version = reader.Get<UnityVersion>();
            var hash = Convert.ToHexString(reader.Get<SerializedType>().OldTypeHash);
            var hasGNFTexture = hash == "1D52BB98AA5F54C67C22C39E8B2E400F";
            var hasExternalMipRelativeOffset = hash is "1D52BB98AA5F54C67C22C39E8B2E400F" or "5390A985F58D5524F95DB240E8789704";
            instance.m_Width = reader.ReadInt32();
            instance.m_Height = reader.ReadInt32();
            var m_CompleteImageSize = reader.ReadInt32();
            if (version.GreaterThanOrEquals(2020)) //2020.1 and up
            {
                var m_MipsStripped = reader.ReadInt32();
            }
            instance.m_TextureFormat = (TextureFormat)reader.ReadInt32();
            if (version.LessThan(5, 2)) //5.2 down
            {
                instance.m_MipMap = reader.ReadBoolean();
            }
            else
            {
                instance.m_MipCount = reader.ReadInt32();
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
            instance.m_TextureSettings = new GLTextureSettings();
            reader.Get<IBundleElementScanner>().TryRead(reader, instance.m_TextureSettings);
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
            var image_data_size = reader.ReadInt32();
            if (image_data_size == 0 && version.GreaterThanOrEquals(5, 3))//5.3.0 and up
            {
                if (hasExternalMipRelativeOffset)
                {
                    var m_externalMipRelativeOffset = reader.ReadUInt32();
                }
                instance.m_StreamData = new StreamingInfo(reader);
            }

            if (!string.IsNullOrEmpty(instance.m_StreamData?.path))
            {
                instance.image_data = ((UIReader)reader).OpenResource(instance.m_StreamData);
            }
            else
            {
                instance.image_data = new PartialStream(reader.BaseStream, image_data_size);
            }
        }

        private static bool CheckHeader(IBundleBinaryReader reader, int offset)
        {
            short value = 0;
            var pos = reader.Position;
            while (value != -1 && reader.Position <= pos + offset)
            {
                value = reader.ReadInt16();
            }
            var isNewHeader = reader.Position - pos == offset;
            reader.Position = pos;
            return isNewHeader;
        }
    }
}
