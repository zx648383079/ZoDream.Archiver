using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SerializedShaderConverter : BundleConverter<SerializedShader>
    {
        public override SerializedShader? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new SerializedShader
            {
                PropInfo = serializer.Deserialize<SerializedProperties>(reader),

                SubShaders = reader.ReadArray<SerializedSubShader>(serializer)
            };


            if (version.GreaterThanOrEquals(2021, 2)) //2021.2 and up
            {
                res.KeywordNames = reader.ReadArray(r => r.ReadAlignedString());
                res.KeywordFlags = reader.ReadArray(r => r.ReadByte());
                reader.AlignStream();
            }

            res.Name = reader.ReadAlignedString();
            res.CustomEditorName = reader.ReadAlignedString();
            res.FallbackName = reader.ReadAlignedString();

            res.Dependencies = reader.ReadArray<SerializedShaderDependency>(serializer);


            if (version.GreaterThanOrEquals(2021, 1)) //2021.1 and up
            {
                res.CustomEditorForRenderPipelines = reader.ReadArray<SerializedCustomEditorForRenderPipeline>(serializer);
            
            }

            res.DisableNoSubshadersMessage = reader.ReadBoolean();
            reader.AlignStream();
            return res;
        }
    }

}
