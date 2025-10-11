using System;
using System.Linq;
using System.Numerics;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Scanners
{

    internal partial class TuanJieElementScanner : BundleConverter
    {
        private readonly Type[] _includeItems = [
            typeof(Mesh), typeof(ClipMuscleConstant), typeof(ConstantBuffer),
            typeof(MatrixParameter), typeof(VectorParameter), typeof(AnimationClip),
            typeof(Texture2D), typeof(GameObject)
        ];
        public override bool CanConvert(Type objectType)
        {
            return _includeItems.Contains(objectType);
        }

        public override object? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            if (objectType == typeof(Mesh))
            {
                return ReadMesh(reader, serializer);
            }
            if (objectType == typeof(Clip))
            {
                return ReadClip(reader, serializer);
            }
            if (objectType == typeof(ClipMuscleConstant))
            {
                return ReadClipMuscleConstant(reader, serializer);
            }
            if (objectType == typeof(ConstantBuffer))
            {
                return ReadConstantBuffer(reader, serializer);
            }
            if (objectType == typeof(MatrixParameter))
            {
                return ReadMatrixParameter(reader, serializer);
            }
            if (objectType == typeof(VectorParameter))
            {
                return ReadVectorParameter(reader, serializer);
            }
            if (objectType == typeof(AnimationClip))
            {
                return ReadAnimationClip(reader, serializer);
            }
            if (objectType == typeof(Texture2D))
            {
                return ReadTexture2D(reader, serializer);
            }
            if (objectType == typeof(GameObject))
            {
                return ReadGameObject(reader, serializer);
            }
            return null;
        }

        private GameObject ReadGameObject(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new GameObject();
            GameObjectConverter.ReadBase(res, reader, serializer, () => {
                var version = reader.Get<Version>();
                if (version.Build >= 13)
                {
                    bool m_HasEditorInfo = reader.ReadBoolean();
                    reader.AlignStream();
                }
            });
            return res;
        }

        private Texture2D ReadTexture2D(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new Texture2D();
            UnityConverter.ReadTexture(res, reader, serializer);
            var version = reader.Get<Version>();
            res.Width = reader.ReadInt32();
            res.Height = reader.ReadInt32();
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
            var m_DataStreamData = new ResourceSource()
            {
                Size = reader.ReadUInt32(),
                Source = reader.ReadAlignedString()
            };
            // res.DataStreamData = new DataStreamingInfo(reader);

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
            if (imageDataSize == 0)//5.3.0 and up
            {
                res.StreamData = serializer.Deserialize<ResourceSource>(reader);
            }

            if (!string.IsNullOrEmpty(res.StreamData.Source))
            {
                var container = reader.Get<ISerializedFile>();
                if (reader.TryGet<IDependencyBuilder>(out var builder))
                {
                    var sourcePath = container.FullPath;
                    var fileId = reader.Get<ObjectInfo>().FileID;
                    builder.AddDependencyEntry(sourcePath,
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


                res.EulerCurves = [];
                res.PositionCurves = [];
                res.ScaleCurves = [];

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
                    if (res.MuscleClipSize > 0)
                    {
                        reader.ReadUInt32(); // not needed
                        res.MuscleClip = serializer.Deserialize<ClipMuscleConstant>(reader);
                        res.StreamData = serializer.Deserialize<ResourceSource>(reader);
                    }
                }
            });
            return res;
        }

        private Clip ReadClip(IBundleBinaryReader reader,
            IBundleSerializer serializer)
        {
            var res = new Clip();
            var version = reader.Get<Version>();
            res.StreamedClip = serializer.Deserialize<StreamedClip>(reader);
            res.DenseClip = UnityConverter.ReadDenseClip(reader, serializer);
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                res.ConstantClip = serializer.Deserialize<ConstantClip>(reader);
            }
            if (IsGuiLongChao)
            {
                res.ACLClip = ReadQuantizedClip(reader, serializer);
                _ = ReadPredictClip(reader, serializer);
            }
            if (version.LessThan(2018, 3)) //2018.3 down
            {
                res.Binding = serializer.Deserialize<ValueArrayConstant>(reader);
            }
            return res;
        }

        private TuanJiePredictClip ReadPredictClip(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new TuanJiePredictClip
            {
                m_FrameCount = reader.ReadInt32(),
                m_CurveCount = reader.ReadUInt32(),
                m_SampleRate = reader.ReadSingle(),
                m_BeginTime = reader.ReadSingle(),
                m_NumStatic = reader.ReadUInt32(),
                m_NumDynamic = reader.ReadUInt32(),
                m_TypeOffset = reader.ReadUInt32(),
                m_IndicesOffset = reader.ReadUInt32(),
                m_StaticOffset = reader.ReadUInt32(),
                m_RangeOffset = reader.ReadUInt32(),
                m_BitCntOffset = reader.ReadUInt32(),
                m_PredictBlockOffset = reader.ReadUInt32(),
                m_ValueOffsetPerCurveOffset = reader.ReadUInt32(),
                m_ValueOffset = reader.ReadUInt32(),
                m_Data = reader.ReadAsStream()
            };
            return res;
        }

        private TuanJieQuantizedClip ReadQuantizedClip(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new TuanJieQuantizedClip
            {
                m_FrameCount = reader.ReadInt32(),
                m_CurveCount = reader.ReadUInt32(),
                m_SampleRate = reader.ReadSingle(),
                m_BeginTime = reader.ReadSingle(),
                m_NumStatic = reader.ReadUInt32(),
                m_NumDynamic = reader.ReadUInt32(),
                m_TypeOffset = reader.ReadUInt32(),
                m_IndicesOffset = reader.ReadUInt32(),
                m_StaticOffset = reader.ReadUInt32(),
                m_FrameSize = reader.ReadUInt32(),
                m_DynamicOffset = reader.ReadUInt32(),
                m_Data = reader.ReadAsStream()
            };
            return res;
        }

        private VectorParameter ReadVectorParameter(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new VectorParameter();
            VectorParameterConverter.ReadBase(ref res, reader, serializer, () => {
                var m_IndexInCB = reader.ReadInt32();
            });
            return res;
        }

        private MatrixParameter ReadMatrixParameter(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new MatrixParameter();
            MatrixParameterConverter.ReadBase(ref res, reader, serializer, () => {
                var m_IndexInCB = reader.ReadInt32();
            });
            return res;
        }

        private ConstantBuffer ReadConstantBuffer(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new ConstantBuffer();
            ConstantBufferConverter.ReadBase(ref res, reader, serializer, () => {
                var m_totalParameterCount = reader.ReadInt32();
            });
            return res;
        }

        private ClipMuscleConstant ReadClipMuscleConstant(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new ClipMuscleConstant();
            var version = reader.Get<Version>();
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

            // 1.4.2
            reader.AlignStream();

            res.IndexArray = reader.ReadArray(r => r.ReadInt32());
            ClipMuscleConstantConverter.ReadBase(res, reader, serializer, () => { });
            return res;
        }

        private Mesh ReadMesh(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new Mesh();
            var version = reader.Get<Version>();
            MeshConverter.ReadBase(res, reader, serializer, () => {
                var m_MeshCompression = 0;
                if (version.GreaterThanOrEquals(2, 6)) //2.6.0 and up
                {
                    if (version.GreaterThanOrEquals(2019)) //2019 and up
                    {
                        var m_BonesAABB = reader.ReadArray<MinMaxAABB>(serializer);

                        var m_VariableBoneCountWeights = reader.ReadArray(r => r.ReadUInt32());
                    }

                    m_MeshCompression = reader.ReadByte();
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
                        res.IndexBufferFormat = reader.ReadArray(RootClusterPage / 4, r => r.ReadUInt32());

                        var ImposterAtlas = reader.ReadArray(_ => reader.ReadUInt16());
                        reader.ReadArray<VGPackedHierarchyNode>(serializer);
                        var HierarchyRootOffsets = reader.ReadArray(_ => reader.ReadUInt32());
                        var PageStreamingStates = reader.ReadArray<VGPageStreamingState>(serializer);

                        var PageDependencies = reader.ReadArray(_ => reader.ReadUInt32());

                    }
                    reader.AlignStream();

                    // add
                    reader.ReadInt32();

                    //Unity fixed it in 2017.3.1p1 and later versions
                    if (
                        version.GreaterThanOrEquals(2017, 4) ||
                        version.Equals(2017, 3, 1, VersionType.Patch, 1) || 
                        (version.Equals(2017, 3) && m_MeshCompression == 0))//2017.3.xfx with no compression
                    {
                        res.IndexFormat = reader.ReadInt32();
                        int m_IndexBuffer_size = reader.ReadInt32();
                        if (res.Use16BitIndices)
                        {
                            res.IndexBufferFormat = reader.ReadArray(m_IndexBuffer_size / 2, r => (uint)r.ReadUInt16());
                            reader.AlignStream();
                        }
                        else
                        {
                            res.IndexBufferFormat = reader.ReadArray(m_IndexBuffer_size / 4, r => r.ReadUInt32());
                        }
                    }

                    
                }

                res.VertexData = serializer.Deserialize<VertexData>(reader);

                if (version.GreaterThanOrEquals(2, 6)) //2.6.0 and later
                {
                    res.CompressedMesh = serializer.Deserialize<CompressedMesh>(reader);
                }


                reader.Position += 24; //AABB m_LocalAABB


                int m_MeshUsageFlags = reader.ReadInt32();

                if (version.GreaterThanOrEquals(2022, 1)) //2022.1 and up
                {
                    int m_CookingOptions = reader.ReadInt32();
                }

                var m_BakedConvexCollisionMesh = reader.ReadArray(r => r.ReadByte());
                reader.AlignStream();
                var m_BakedTriangleCollisionMesh = reader.ReadArray(r => r.ReadByte());
                reader.AlignStream();

                var m_MeshMetrics = new float[2];
                m_MeshMetrics[0] = reader.ReadSingle();
                m_MeshMetrics[1] = reader.ReadSingle();

            });
            return res;
        }
    }


}
