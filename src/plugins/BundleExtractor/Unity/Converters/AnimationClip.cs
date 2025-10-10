using System;
using UnityEngine;
using UnityEngine.Document;
using ZoDream.BundleExtractor.Unity.Document;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{

    internal class AnimationClipConverter : BundleConverter<AnimationClip>, ITypeTreeConverter
    {
        public static void ReadBase(AnimationClip res, IBundleBinaryReader reader, 
            IBundleSerializer serializer, Action cb)
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
            if (version.Major >= 5)//5.0 and up
            {
                res.Legacy = reader.ReadBoolean();
            }
            else if (version.Major >= 4)//4.0 and up
            {
                res.AnimationType = (AnimationType)reader.ReadInt32();
                if (res.AnimationType == AnimationType.Legacy)
                {
                    res.Legacy = true;
                }
            }
            else
            {
                res.Legacy = true;
            }
            cb.Invoke();
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                res.ClipBindingConstant = serializer.Deserialize<AnimationClipBindingConstant>(reader);
            }
            if (version.GreaterThanOrEquals(2018, 3)) //2018.3 and up
            {
                var m_HasGenericRootTransform = reader.ReadBoolean();
                var m_HasMotionFloatCurves = reader.ReadBoolean();
                reader.AlignStream();
            }
            res.Events = reader.ReadArray<AnimationEvent>(serializer);
            if (version.Major >= 2017) //2017 and up
            {
                reader.AlignStream();
            }
            
        }

        public override AnimationClip? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new AnimationClip();
            ReadBase(res, reader, serializer, () => {
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
                    res.MuscleClipSize = reader.ReadUInt32();
                    res.MuscleClip = serializer.Deserialize<ClipMuscleConstant>(reader);
                }
            });
            return res;
        }

        public object? Read(IBundleBinaryReader reader, Type target, VirtualDocument typeMaps)
        {
            var res = new AnimationClip();
            var container = reader.Get<ISerializedFile>();
            new DocumentReader(container).Read(typeMaps, reader, res);
            return res;
        }
    }
}
