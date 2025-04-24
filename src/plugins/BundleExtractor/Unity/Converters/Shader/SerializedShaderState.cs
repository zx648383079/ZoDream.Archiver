using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SerializedShaderStateConverter : BundleConverter<SerializedShaderState>
    {
        public override SerializedShaderState? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new SerializedShaderState();
            ReadBase(res, reader, serializer, () => { });
            res.Lighting = reader.ReadBoolean();
            reader.AlignStream();
            return res;
        }
        public static void ReadBase(SerializedShaderState res, IBundleBinaryReader reader, 
            IBundleSerializer serializer, Action cb)
        {
            var version = reader.Get<Version>();

            res.Name = reader.ReadAlignedString();
            res.RtBlend = reader.ReadArray(8, _ => serializer.Deserialize<SerializedShaderRTBlendState>(reader));

            res.RtSeparateBlend = reader.ReadBoolean();
            reader.AlignStream();
            if (version.GreaterThanOrEquals(2017, 2)) //2017.2 and up
            {
                res.ZClip = serializer.Deserialize<SerializedShaderFloatValue>(reader);
            }
            res.ZTest = serializer.Deserialize<SerializedShaderFloatValue>(reader);
            res.ZWrite = serializer.Deserialize<SerializedShaderFloatValue>(reader);
            res.Culling = serializer.Deserialize<SerializedShaderFloatValue>(reader);
            if (version.GreaterThanOrEquals(2020, 1)) //2020.1 and up
            {
                res.Conservative = serializer.Deserialize<SerializedShaderFloatValue>(reader);
            }
            res.OffsetFactor = serializer.Deserialize<SerializedShaderFloatValue>(reader);
            res.OffsetUnits = serializer.Deserialize<SerializedShaderFloatValue>(reader);
            res.AlphaToMask = serializer.Deserialize<SerializedShaderFloatValue>(reader);
            res.StencilOp = serializer.Deserialize<SerializedStencilOp>(reader);
            res.StencilOpFront = serializer.Deserialize<SerializedStencilOp>(reader);
            res.StencilOpBack = serializer.Deserialize<SerializedStencilOp>(reader);
            res.StencilReadMask = serializer.Deserialize<SerializedShaderFloatValue>(reader);
            res.StencilWriteMask = serializer.Deserialize<SerializedShaderFloatValue>(reader);
            res.StencilRef = serializer.Deserialize<SerializedShaderFloatValue>(reader);
            res.FogStart = serializer.Deserialize<SerializedShaderFloatValue>(reader);
            res.FogEnd = serializer.Deserialize<SerializedShaderFloatValue>(reader);
            res.FogDensity = serializer.Deserialize<SerializedShaderFloatValue>(reader);
            res.FogColor = serializer.Deserialize<SerializedShaderVectorValue>(reader);
            res.FogMode = (FogMode)reader.ReadInt32();
            res.GpuProgramID = reader.ReadInt32();
            res.Tags = serializer.Deserialize<SerializedTagMap>(reader);
            res.LOD = reader.ReadInt32();
            cb.Invoke();
        }
    }

}
