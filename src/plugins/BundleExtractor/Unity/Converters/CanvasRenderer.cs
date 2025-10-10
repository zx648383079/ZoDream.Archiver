using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class CanvasRendererConverter : BundleConverter<CanvasRenderer>
    {
        public override CanvasRenderer? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new CanvasRenderer();
        }
    }
}
