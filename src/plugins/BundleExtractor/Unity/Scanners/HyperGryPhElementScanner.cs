using System;
using System.Collections.Generic;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    public class HyperGryPhElementScanner(string package) : IBundleElementScanner
    {

        public bool IsArkNightsEndfield => package.Contains("endfield");

        public bool IsExAstris => package.Contains("exa");

        public bool TryRead(IBundleBinaryReader reader, object instance)
        {
            if (IsArkNightsEndfield && instance is UnityTexEnv e)
            {
                e.Read(reader);
                var m_UVSetIndex = reader.ReadInt32();
                return true;
            }
            if (IsExAstris && instance is GLTextureSettings g)
            {
                CreateInstance(reader, g);
                return true;
            }
            if (instance is Mesh m)
            {
                CreateInstance(reader, m);
                return true;
            }
            if (instance is Clip c)
            {
                CreateInstance(reader, c);
                return true;
            }
            if (instance is BlendTreeNodeConstant bt)
            {
                CreateInstance(reader, bt);
                return true;
            }
            if (instance is AnimationClip ac)
            {
                CreateInstance(reader, ac);
                return true;
            }
            if (instance is ACLDenseClip adc)
            {
                CreateInstance(reader, adc);
                return true;
            }
            if (instance is StateConstant sc)
            {
                CreateInstance(reader, sc);
                return true;
            }
            if (IsArkNightsEndfield && instance is UIRenderer r)
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

        private void CreateInstance(IBundleBinaryReader reader,
            StateConstant instance)
        {
            instance.ReadBase(reader);
            if (IsArkNightsEndfield)
            {
                var m_SyncGroupID = reader.ReadUInt32();
                var m_SyncGroupRole = reader.ReadUInt32();
            }

            reader.AlignStream();
        }
        private void CreateInstance(IBundleBinaryReader reader,
            BlendTreeNodeConstant instance)
        {
            instance.ReadBase(reader);
            var version = reader.Get<UnityVersion>();
            if (version.GreaterThanOrEquals(4, 1, 3)) //4.1.3 and up
            {
                instance.m_CycleOffset = reader.ReadSingle();
                if (IsArkNightsEndfield)
                {
                    var m_StateNameHash = reader.ReadUInt32();
                }
                instance.m_Mirror = reader.ReadBoolean();
                reader.AlignStream();
            }
        }
        private void CreateInstance(IBundleBinaryReader reader,
            Clip instance)
        {
            var version = reader.Get<UnityVersion>();
            instance.m_StreamedClip = new StreamedClip(reader);
            if (IsArkNightsEndfield || IsExAstris)
            {
                var a = new ACLDenseClip();
                CreateInstance(reader, a);
                instance.m_DenseClip = a;
            }
            else
            {
                instance.m_DenseClip = new();
                TryRead(reader, instance.m_DenseClip);
            }
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                instance.m_ConstantClip = new ConstantClip(reader);
            }
            if (version.LessThan(2018, 3)) //2018.3 down
            {
                instance.m_Binding = new ValueArrayConstant(reader);
            }
        }
        private void CreateInstance(IBundleBinaryReader reader,
            ACLDenseClip instance)
        {
            instance.ReadBase(reader, () => {
                if (IsArkNightsEndfield)
                {
                    instance.m_ACLArray = reader.ReadArray(r => r.ReadByte());
                    reader.AlignStream();
                    instance.m_PositionFactor = reader.ReadSingle();
                    instance.m_EulerFactor = reader.ReadSingle();
                    instance.m_ScaleFactor = reader.ReadSingle();
                    instance.m_FloatFactor = reader.ReadSingle();
                    instance.m_nPositionCurves = reader.ReadUInt32();
                    instance.m_nRotationCurves = reader.ReadUInt32();
                    instance.m_nEulerCurves = reader.ReadUInt32();
                    instance.m_nScaleCurves = reader.ReadUInt32();
                }
                else if (IsExAstris)
                {
                    instance.m_nPositionCurves = reader.ReadUInt32();
                    instance.m_nRotationCurves = reader.ReadUInt32();
                    instance.m_nEulerCurves = reader.ReadUInt32();
                    instance.m_nScaleCurves = reader.ReadUInt32();
                    instance.m_nGenericCurves = reader.ReadUInt32();
                    instance.m_PositionFactor = reader.ReadSingle();
                    instance.m_EulerFactor = reader.ReadSingle();
                    instance.m_ScaleFactor = reader.ReadSingle();
                    instance.m_FloatFactor = reader.ReadSingle();
                    instance.m_ACLArray = reader.ReadArray(r => r.ReadByte());
                    reader.AlignStream();
                }
            });
        }
        private void CreateInstance(IBundleBinaryReader reader,
            AnimationClip instance)
        {
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

                if (IsExAstris)
                {
                    var m_aclType = reader.ReadInt32();
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
                if (IsArkNightsEndfield)
                {
                    var m_aclType = reader.ReadInt32();
                }
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
        private void CreateInstance(IBundleBinaryReader reader,
            Mesh instance)
        {
            var version = reader.Get<UnityVersion>();
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

                        var m_KeepVertices = reader.ReadBoolean();
                        var m_KeepIndices = reader.ReadBoolean();

                        if (IsArkNightsEndfield)
                        {
                            var m_CollisionMeshOnly = reader.ReadBoolean();
                            instance.m_CollisionMeshBaked = reader.ReadBoolean();
                            var m_CollisionMeshConvex = reader.ReadBoolean();
                        }
                    }
                    reader.AlignStream();

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

                if (IsExAstris)
                {
                    var m_ColliderType = reader.ReadInt32();
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
                }


                if (version.GreaterThanOrEquals(2018, 2)) //2018.2 and up
                {
                    var m_MeshMetrics = new float[2];
                    m_MeshMetrics[0] = reader.ReadSingle();
                    m_MeshMetrics[1] = reader.ReadSingle();
                    if (IsArkNightsEndfield)
                    {
                        var m_MeshMetrics2 = reader.ReadSingle();
                    }
                }

            });
        }
        private void CreateInstance(IBundleBinaryReader reader, UIRenderer instance)
        {
            instance.ReadBase(reader);
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
                    var m_Enabled = reader.ReadBoolean();
                    var m_CastShadows = reader.ReadByte();
                    var m_ReceiveShadows = reader.ReadByte();
                    if (version.GreaterThanOrEquals(2017, 2)) //2017.2 and up
                    {
                        var m_DynamicOccludee = reader.ReadByte();
                    }
                    if (version.Major >= 2021) //2021.1 and up
                    {
                        var m_StaticShadowCaster = reader.ReadByte();
                        var m_RealtimeShadowCaster = reader.ReadByte();
                        var m_SubMeshRenderMode = reader.ReadByte();
                        var m_CharacterIndex = reader.ReadByte();
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
                if (version.Major >= 2018) //2018 and up
                {
                    var m_RenderingLayerMask = reader.ReadUInt32();
                }

                if (version.GreaterThanOrEquals(2018, 3)) //2018.3 and up
                {
                    var m_RendererPriority = reader.ReadInt32();
                }

                var m_LightmapIndex = reader.ReadUInt16();
                var m_LightmapIndexDynamic = reader.ReadUInt16();

            }

            if (version.Major >= 3) //3.0 and up
            {
                var m_LightmapTilingOffset = reader.ReadVector4();
            }

            if (version.Major >= 5) //5.0 and up
            {
                var m_LightmapTilingOffsetDynamic = reader.ReadVector4();
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
            }
        }
        private void CreateInstance(IBundleBinaryReader reader, GLTextureSettings instance)
        {
            var version = reader.Get<UnityVersion>();

            instance.m_FilterMode = reader.ReadInt32();
            instance.m_Aniso = reader.ReadInt32();
            instance.m_MipBias = reader.ReadSingle();
            var m_TextureGroup = reader.ReadInt32();
            if (version.Major >= 2017)//2017.x and up
            {
                instance.m_WrapMode = reader.ReadInt32(); //m_WrapU
                int m_WrapV = reader.ReadInt32();
                int m_WrapW = reader.ReadInt32();
            }
            else
            {
                instance.m_WrapMode = reader.ReadInt32();
            }
        }
    }
}
