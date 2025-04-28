using System;
using System.Collections.Specialized;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal static class UnityConverter
    {
        internal static IBundleConverter[] Converters = [
            new AnimationConverter(),
            new AnimationClipConverter(),
            new AnimatorConverter(),
            new AnimatorControllerConverter(),
            new AnimatorOverrideControllerConverter(),
            new AssetInfoConverter(),
            new AssetBundleConverter(),
            new AudioClipConverter(),
            new AvatarConverter(),
            new BuildSettingsConverter(),
            new FontConverter(),
            new GameObjectConverter(),
            new IndexConverter(),
            new IndexObjectConverter(),
            new MaterialConverter(),
            new MeshConverter(),
            new MeshFilterConverter(),
            new MeshRendererConverter(),
            new MonoBehaviourConverter(),
            new MovieTextureConverter(),
            new PlayerSettingsConverter(),
            new PPtrConverter(),
            new StaticBatchInfoConverter(),
            new ResourceManagerConverter(),
            new ShaderConverter(),
            new SkinnedMeshRendererConverter(),
            new SpriteConverter(),
            new SpriteAtlasConverter(),
            new StreamingInfoConverter(),
            new TextAssetConverter(),
            new GLTextureSettingsConverter(),
            new Texture2DConverter(),
            new Texture2DArrayConverter(),
            new TransformConverter(),
            new VideoClipConverter(),
            new AABBConverter(),
            new ACLDenseClipConverter(),
            new AclTransformTrackIDToBindingCurveIDConverter(),
            new AnimationClipBindingConstantConverter(),
            new AnimationClipOverrideConverter(),
            new AnimationEventConverter(),
            new Blend1dDataConstantConverter(),
            new Blend2dDataConstantConverter(),
            new BlendDirectDataConstantConverter(),
            new BlendTreeConstantConverter(),
            new BlendTreeNodeConstantConverter(),
            new ClipConverter(),
            new ClipMuscleConstantConverter(),
            new CompressedAnimationCurveConverter(),
            new ConditionConstantConverter(),
            new ConstantClipConverter(),
            new ControllerConstantConverter(),
            new FloatCurveConverter(),
            new GenericBindingConverter(),
            new GIACLClipConverter(),
            new HandPoseConverter(),
            new HumanGoalConverter(),
            new HumanPoseConverter(),
            new HumanPoseMaskConverter(),
            new LayerConstantConverter(),
            new LeafInfoConstantConverter(),
            new LnDACLClipConverter(),
            new MotionNeighborListConverter(),
            new PackedFloatVectorConverter(),
            new PackedIntVectorConverter(),
            new PackedQuatVectorConverter(),
            new PPtrCurveConverter(),
            new PPtrKeyframeConverter(),
            new QuaternionCurveConverter(),
            new SelectorStateConstantConverter(),
            new SelectorTransitionConstantConverter(),
            new SkeletonMaskConverter(),
            new SkeletonMaskElementConverter(),
            new StateConstantConverter(),
            new StateMachineConstantConverter(),
            new StreamedClipConverter(),
            new StreamedCurveKeyConverter(),
            new StreamedFrameConverter(),
            new TransitionConstantConverter(),
            new ValueArrayConverter(),
            new ValueArrayConstantConverter(),
            new ValueConstantConverter(),
            new ValueDeltaConverter(),
            new Vector3CurveConverter(),
            new PropertySheetConverter(),
            new TexEnvConverter(),
            new BlendShapeDataConverter(),
            new BlendShapeVertexConverter(),
            new BoneWeights4Converter(),
            new ChannelInfoConverter(),
            new CompressedMeshConverter(),
            new MeshBlendShapeConverter(),
            new MeshBlendShapeChannelConverter(),
            new MinMaxAABBConverter(),
            new StreamInfoConverter(),
            new SubMeshConverter(),
            new VertexDataConverter(),
            new VGPackedHierarchyNodeConverter(),
            new VGPageStreamingStateConverter(),
            new BufferBindingConverter(),
            new ConstantBufferConverter(),
            new Hash128Converter(),
            new MatrixParameterConverter(),
            new ParserBindChannelsConverter(),
            new SamplerParameterConverter(),
            new SerializedCustomEditorForRenderPipelineConverter(),
            new SerializedPassConverter(),
            new SerializedPlayerSubProgramConverter(),
            new SerializedProgramConverter(),
            new SerializedProgramParametersConverter(),
            new SerializedPropertiesConverter(),
            new SerializedPropertyConverter(),
            new SerializedShaderConverter(),
            new SerializedShaderDependencyConverter(),
            new SerializedShaderFloatValueConverter(),
            new SerializedShaderRTBlendStateConverter(),
            new SerializedShaderStateConverter(),
            new SerializedShaderVectorValueConverter(),
            new SerializedStencilOpConverter(),
            new SerializedSubProgramConverter(),
            new SerializedSubShaderConverter(),
            new SerializedTagMapConverter(),
            new SerializedTexturePropertyConverter(),
            new ShaderBindChannelConverter(),
            new StructParameterConverter(),
            new TextureParameterConverter(),
            new UAVParameterConverter(),
            new VectorParameterConverter(),
            new AvatarConstantConverter(),
            new AxesConverter(),
            new ColliderConverter(),
            new HandConverter(),
            new HandleConverter(),
            new HumanConverter(),
            new LimitConverter(),
            new NodeConverter(),
            new SkeletonConverter(),
            new SkeletonPoseConverter(),
            new SecondarySpriteTextureConverter(),
            new SpriteAtlasDataConverter(),
            new SpriteRenderDataConverter(),
            new SpriteSettingsConverter(),
            new SpriteVertexConverter(),

            new MonoScriptConverter(),
        ];
        public static void ReadTexture(Texture res, IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            var version = reader.Get<Version>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            res.Name = reader.ReadAlignedString();
            if (version.GreaterThanOrEquals(2017, 3)) //2017.3 and up
            {
                var m_ForcedFallbackFormat = reader.ReadInt32();
                var m_DownscaleFallback = reader.ReadBoolean();
                if (version.GreaterThanOrEquals(2020, 2)) //2020.2 and up
                {
                    var m_IsAlphaChannelOptional = reader.ReadBoolean();
                }
                reader.AlignStream();
            }
        }

        public static DenseClip ReadDenseClip(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new DenseClip();
            ReadDenseClip(res, reader, serializer);
            return res;
        }
        public static void ReadDenseClip(DenseClip res, IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            res.FrameCount = reader.ReadInt32();
            res.CurveCount = reader.ReadUInt32();
            res.SampleRate = reader.ReadSingle();
            res.BeginTime = reader.ReadSingle();
            res.SampleArray = reader.ReadArray(r => r.ReadSingle());
        }

        public static AnimationCurve<T> ReadAnimationCurve<T>(
            IBundleBinaryReader reader, 
            Func<T> readerFunc)
        {
            var res = new AnimationCurve<T>();
            var version = reader.Get<Version>();
            res.Curve = reader.ReadArray(_ => ReadKeyframe<T>(reader, readerFunc));

            res.PreInfinity = reader.ReadInt32();
            res.PostInfinity = reader.ReadInt32();
            if (version.GreaterThanOrEquals(5, 3))//5.3 and up
            {
                res.RotationOrder = reader.ReadInt32();
            }
            return res;
        }

        public static Keyframe<T> ReadKeyframe<T>(IBundleBinaryReader reader, Func<T> readerFunc)
        {
            var res = new Keyframe<T>
            {
                Time = reader.ReadSingle(),
                Value = readerFunc(),
                InSlope = readerFunc(),
                OutSlope = readerFunc()
            };
            if (reader.Get<Version>().Major >= 2018) //2018 and up
            {
                res.WeightedMode = reader.ReadInt32();
                res.InWeight = readerFunc();
                res.OutWeight = readerFunc();
            }
            return res;
        }


        public static OrderedDictionary? ToType(int entryId, ISerializedFile resource)
        {
            return ToType(resource.TypeItems[resource.Get(entryId).SerializedTypeIndex].OldType,
                entryId, resource);
        }

        public static OrderedDictionary? ToType(TypeTree m_Type, int entryId, ISerializedFile resource)
        {
            if (m_Type != null)
            {
                return TypeTreeHelper.ReadType(m_Type, resource.OpenRead(entryId));
            }
            return null;
        }
    }
}
