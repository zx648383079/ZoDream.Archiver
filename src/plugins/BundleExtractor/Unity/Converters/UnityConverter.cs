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
