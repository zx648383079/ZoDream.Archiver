using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SerializedCustomEditorForRenderPipelineConverter : BundleConverter<SerializedCustomEditorForRenderPipeline>
    {
        public override SerializedCustomEditorForRenderPipeline Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                CustomEditorName = reader.ReadAlignedString(),
                RenderPipelineType = reader.ReadAlignedString(),
            };
        }
    }

}
