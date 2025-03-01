using System.Collections.Generic;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class TuanJieElementScanner
    {

        private void CreateInstance(IBundleBinaryReader reader, ClipMuscleConstant instance)
        {
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
            instance.m_AverageSpeed = version.GreaterThanOrEquals(5, 4) ? reader.ReadVector3Or4() :
                UnityReaderExtension.Parse(reader.ReadVector4());//5.4 and up
            instance.m_Clip = new Clip();
            TryRead(reader, instance.m_Clip);
            instance.m_StartTime = reader.ReadSingle();
            instance.m_StopTime = reader.ReadSingle();
            instance.m_OrientationOffsetY = reader.ReadSingle();
            instance.m_Level = reader.ReadSingle();
            instance.m_CycleOffset = reader.ReadSingle();
            instance.m_AverageAngularSpeed = reader.ReadSingle();

            // 1.4.2
            reader.AlignStream();

            instance.m_IndexArray = reader.ReadArray(r => r.ReadInt32());
            instance.ReadBase(reader);
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

                instance.m_EulerCurves = [];
                instance.m_PositionCurves = [];
                instance.m_ScaleCurves = [];


                

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
                    if (instance.m_MuscleClipSize > 0)
                    {
                        reader.ReadUInt32(); // not needed
                        instance.m_MuscleClip = new();
                        TryRead(reader, instance.m_MuscleClip);
                        instance.m_StreamData = new StreamingInfo(reader);
                    }
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

                        var m_LightmapUseUV1 = reader.ReadInt32();
                        var m_fileScale = reader.ReadSingle();

                        var NumInputTriangles = reader.ReadUInt32();
                        var NumInputVertices = reader.ReadUInt32();
                        var NumInputMeshes = reader.ReadUInt16();
                        var NumInputTexCoords = reader.ReadUInt16();
                        var ResourceFlags = reader.ReadUInt32();

                        var RootClusterPage = reader.ReadInt32();
                        instance.m_IndexBuffer = reader.ReadArray(RootClusterPage / 4, (r, _) => r.ReadUInt32());

                        var ImposterAtlas = reader.ReadInt32();
                        for (int i = 0; i < ImposterAtlas; i++)
                        {
                            reader.ReadUInt16();
                        }
                        var HierarchyNodes = reader.ReadInt32();
                        for (int i = 0; i < HierarchyNodes; i++)
                        {
                            new VGPackedHierarchyNode(reader);
                        }
                        var HierarchyRootOffsets = reader.ReadInt32();
                        for (int i = 0; i < HierarchyRootOffsets; i++)
                        {
                            reader.ReadUInt32();
                        }
                        var PageStreamingStates = reader.ReadInt32();
                        for (int i = 0; i < PageStreamingStates; i++)
                        {
                            new VGPageStreamingState(reader);
                        }
                        var PageDependencies = reader.ReadInt32();
                        for (int i = 0; i < PageDependencies; i++)
                        {
                            reader.ReadUInt32();
                        }

                    }
                    reader.AlignStream();

                    // add
                    reader.ReadInt32();

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
                }


                if (version.GreaterThanOrEquals(2018, 2)) //2018.2 and up
                {
                    var m_MeshMetrics = new float[2];
                    m_MeshMetrics[0] = reader.ReadSingle();
                    m_MeshMetrics[1] = reader.ReadSingle();
                }

            });
        }

        private void CreateInstance(IBundleBinaryReader reader,
            MatrixParameter instance)
        {
            instance.ReadBase(reader, () => {
                var m_IndexInCB = reader.ReadInt32();
            });
        }

        private void CreateInstance(IBundleBinaryReader reader,
            ConstantBuffer instance)
        {
            instance.ReadBase(reader, () => {
                var m_totalParameterCount = reader.ReadInt32();
            });
        }
        private void CreateInstance(IBundleBinaryReader reader,
            VectorParameter instance)
        {
            instance.ReadBase(reader, () => {
                var m_IndexInCB = reader.ReadInt32();
            });
        }
        private void CreateInstance(IBundleBinaryReader reader,
           Texture2D instance)
        {
            instance.ReadBase(reader);
            var version = reader.Get<UnityVersion>();
            instance.m_Width = reader.ReadInt32();
            instance.m_Height = reader.ReadInt32();
            var m_CompleteImageSize = reader.ReadInt32();
            if (version.GreaterThanOrEquals(2020)) //2020.1 and up
            {
                var m_MipsStripped = reader.ReadInt32();
            }
            // ADD
            var m_WebStreaming = reader.ReadBoolean();
            reader.AlignStream();

            var m_PriorityLevel = reader.ReadInt32();
            var m_UploadedMode = reader.ReadInt32();
            var m_DataStreamData_size = reader.ReadUInt32();
            var m_DataStreamData_path = reader.ReadAlignedString();
            // instance.m_DataStreamData = new DataStreamingInfo(reader);

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
            if (image_data_size == 0)//5.3.0 and up
            {
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

        private void CreateInstance(IBundleBinaryReader reader,
           GameObject instance)
        {
            instance.ReadBase(reader, () => {
                var version = reader.Get<UnityVersion>();
                if (version.Build >= 13)
                {
                    bool m_HasEditorInfo = reader.ReadBoolean();
                    reader.AlignStream();
                }
            });
        }
    }
}
