using System;
using System.Linq;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Scanners
{

    public partial class HyperGryPhElementScanner : BundleConverter, IBundlePipelineConverter<Renderer>
    {
        private readonly Type[] _includeItems = [
            typeof(TexEnv),
            typeof(GLTextureSettings),
            typeof(Mesh),
            typeof(Clip),
            typeof(BlendTreeNodeConstant),
            typeof(AnimationClip),
            typeof(ACLDenseClip),
            typeof(StateConstant),
            typeof(Renderer),
            ];

        public override bool CanConvert(Type objectType)
        {
            return _includeItems.Contains(objectType);
        }



        public override object? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            if (objectType == typeof(TexEnv))
            {
                return ReadTexEnv(reader, serializer);
            }
            if (objectType == typeof(GLTextureSettings))
            {
                return ReadGLTextureSettings(reader, serializer);
            }
            if (objectType == typeof(Mesh))
            {
                return ReadMesh(reader, serializer);
            }
            if (objectType == typeof(Clip))
            {
                return ReadClip(reader, serializer);
            }
            if (objectType == typeof(BlendTreeNodeConstant))
            {
                return ReadBlendTreeNodeConstant(reader, serializer);
            }
            if (objectType == typeof(AnimationClip))
            {
                return ReadAnimationClip(reader, serializer);
            }
            if (objectType == typeof(ACLDenseClip))
            {
                return ReadACLDenseClip(reader, serializer);
            }
            if (objectType == typeof(StateConstant))
            {
                return ReadStateConstant(reader, serializer);
            }
            if (objectType == typeof(Renderer))
            {
            }
            return null;
        }
        private TexEnv ReadTexEnv(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new TexEnv();
            TexEnvConverter.ReadBase(res, reader, serializer, () => { });
            if (IsArkNightsEndfield)
            {
                var m_UVSetIndex = reader.ReadInt32();
            }
            return res;
        }
        private StateConstant ReadStateConstant(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new StateConstant();
            StateConstantConverter.ReadBase(res, reader, serializer, () => { });
            if (IsArkNightsEndfield)
            {
                var m_SyncGroupID = reader.ReadUInt32();
                var m_SyncGroupRole = reader.ReadUInt32();
            }

            reader.AlignStream();
            return res;
        }
        private BlendTreeNodeConstant ReadBlendTreeNodeConstant(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new BlendTreeNodeConstant();
            BlendTreeNodeConstantConverter.ReadBase(res, reader, serializer, () => { });
            var version = reader.Get<Version>();
            if (version.GreaterThanOrEquals(4, 1, 3)) //4.1.3 and up
            {
                res.CycleOffset = reader.ReadSingle();
                if (IsArkNightsEndfield)
                {
                    var m_StateNameHash = reader.ReadUInt32();
                }
                res.Mirror = reader.ReadBoolean();
                reader.AlignStream();
            }
            return res;
        }
        private Clip ReadClip(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new Clip();
            var version = reader.Get<Version>();
            res.StreamedClip = serializer.Deserialize<StreamedClip>(reader);
            if (IsArkNightsEndfield || IsExAstris)
            {
                res.DenseClip = serializer.Deserialize<ACLDenseClip>(reader);
            }
            else
            {
                res.DenseClip = serializer.Deserialize<DenseClip>(reader);
            }
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                res.ConstantClip = serializer.Deserialize<ConstantClip>(reader);
            }
            if (version.LessThan(2018, 3)) //2018.3 down
            {
                res.Binding = serializer.Deserialize<ValueArrayConstant>(reader);
            }
            return res;
        }
        private ACLDenseClip ReadACLDenseClip(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new ACLDenseClip();
            ACLDenseClipConverter.ReadBase(res, reader, serializer, () => {
                if (IsArkNightsEndfield)
                {
                    res.ACLArray = reader.ReadArray(r => r.ReadByte());
                    reader.AlignStream();
                    res.PositionFactor = reader.ReadSingle();
                    res.EulerFactor = reader.ReadSingle();
                    res.ScaleFactor = reader.ReadSingle();
                    res.FloatFactor = reader.ReadSingle();
                    res.nPositionCurves = reader.ReadUInt32();
                    res.nRotationCurves = reader.ReadUInt32();
                    res.nEulerCurves = reader.ReadUInt32();
                    res.nScaleCurves = reader.ReadUInt32();
                }
                else if (IsExAstris)
                {
                    res.nPositionCurves = reader.ReadUInt32();
                    res.nRotationCurves = reader.ReadUInt32();
                    res.nEulerCurves = reader.ReadUInt32();
                    res.nScaleCurves = reader.ReadUInt32();
                    res.nGenericCurves = reader.ReadUInt32();
                    res.PositionFactor = reader.ReadSingle();
                    res.EulerFactor = reader.ReadSingle();
                    res.ScaleFactor = reader.ReadSingle();
                    res.FloatFactor = reader.ReadSingle();
                    res.ACLArray = reader.ReadArray(r => r.ReadByte());
                    reader.AlignStream();
                }
            });
            return res;
        }
        private AnimationClip ReadAnimationClip(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new AnimationClip();
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


                if (IsExAstris)
                {
                    var m_aclType = reader.ReadInt32();
                }

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
                if (IsArkNightsEndfield)
                {
                    var m_aclType = reader.ReadInt32();
                }
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
        private Mesh ReadMesh(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new Mesh();
            var version = reader.Get<Version>();
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

                        var m_KeepVertices = reader.ReadBoolean();
                        var m_KeepIndices = reader.ReadBoolean();

                        if (IsArkNightsEndfield)
                        {
                            var m_CollisionMeshOnly = reader.ReadBoolean();
                            res.CollisionMeshBaked = reader.ReadBoolean();
                            var m_CollisionMeshConvex = reader.ReadBoolean();
                        }
                    }
                    reader.AlignStream();

                    //Unity fixed it in 2017.3.1p1 and later versions
                    if (version.GreaterThanOrEquals(2017, 3, 1, VersionType.Patch, 1) && m_MeshCompression == 0)//2017.3.xfx with no compression
                    {
                        var m_IndexFormat = reader.ReadInt32();
                        res.Use16BitIndices = m_IndexFormat == 0;
                    }

                    int m_IndexBuffer_size = reader.ReadInt32();
                    if (res.Use16BitIndices)
                    {
                        res.IndexBuffer = reader.ReadArray(m_IndexBuffer_size / 2, r => (uint)r.ReadUInt16());
                        reader.AlignStream();
                    }
                    else
                    {
                        res.IndexBuffer = reader.ReadArray(m_IndexBuffer_size / 4, r => r.ReadUInt32());
                    }
                }

                if (version.LessThan(3, 5)) //3.4.2 and earlier
                {
                    res.VertexCount = reader.ReadInt32();
                    res.Vertices = reader.ReadArray(res.VertexCount * 3, r => r.ReadSingle()); //Vector3

                    res.Skin = reader.ReadArray<BoneWeights4>(serializer);

                    res.BindPose = reader.ReadMatrixArray();

                    res.UV0 = reader.ReadArray(reader.ReadInt32() * 2, r => r.ReadSingle()); //Vector2

                    res.UV1 = reader.ReadArray(reader.ReadInt32() * 2, r => r.ReadSingle()); //Vector2

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
            return res;
        }

        public void Read(ref Renderer res, IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            if (!IsArkNightsEndfield)
            {
                return;
            }
            RendererConverter.ReadBase(res, reader, serializer);
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
        private GLTextureSettings ReadGLTextureSettings(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new GLTextureSettings();
            GLTextureSettingsConverter.ReadBase(ref res, reader, serializer, () => {
                if (IsExAstris)
                {
                    var m_TextureGroup = reader.ReadInt32();
                }
            });
            return res;
        }
    }
}
