﻿using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class AnimationClipOverrideConverter : BundleConverter<AnimationClipOverride>
    {
        public override AnimationClipOverride? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                OriginalClip = reader.ReadPPtr<AnimationClip>(serializer),
                OverrideClip = reader.ReadPPtr<AnimationClip>(serializer)
            };
        }
    }
}
