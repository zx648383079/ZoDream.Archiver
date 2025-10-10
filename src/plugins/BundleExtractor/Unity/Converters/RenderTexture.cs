using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class RenderTextureConverter : BundleConverter<RenderTexture>
    {
        public override RenderTexture? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new RenderTexture();
        }
    }
}
