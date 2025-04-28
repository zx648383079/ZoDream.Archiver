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
                VectorParams = reader.ReadArray<VectorParameter>(serializer),
                MatrixParams = reader.ReadArray<MatrixParameter>(serializer),
                TextureParams = reader.ReadArray<TextureParameter>(serializer),
                BufferParams = reader.ReadArray<BufferBinding>(serializer),
                ConstantBuffers = reader.ReadArray<ConstantBuffer>(serializer),
                ConstantBufferBindings = reader.ReadArray<BufferBinding>(serializer),
                UAVParams = reader.ReadArray<UAVParameter>(serializer),
                Samplers = reader.ReadArray<SamplerParameter>(serializer)
            };
            return res;
        }
    }

}
