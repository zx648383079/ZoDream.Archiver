using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SerializedProgramParametersConverter : BundleConverter<SerializedProgramParameters>
    {
        public override SerializedProgramParameters? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new SerializedProgramParameters
            {
                VectorParams = reader.ReadArray(_ => serializer.Deserialize<VectorParameter>(reader)),
                MatrixParams = reader.ReadArray(_ => serializer.Deserialize<MatrixParameter>(reader)),
                TextureParams = reader.ReadArray(_ => serializer.Deserialize<TextureParameter>(reader)),
                BufferParams = reader.ReadArray(_ => serializer.Deserialize<BufferBinding>(reader)),
                ConstantBuffers = reader.ReadArray(_ => serializer.Deserialize<ConstantBuffer>(reader)),
                ConstantBufferBindings = reader.ReadArray(_ => serializer.Deserialize<BufferBinding>(reader)),
                UAVParams = reader.ReadArray(_ => serializer.Deserialize<UAVParameter>(reader)),
                Samplers = reader.ReadArray(_ => serializer.Deserialize<SamplerParameter>(reader))
            };
            return res;
        }
    }

}
