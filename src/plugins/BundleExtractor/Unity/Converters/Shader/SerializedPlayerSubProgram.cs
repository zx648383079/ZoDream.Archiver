using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SerializedPlayerSubProgramConverter : BundleConverter<SerializedPlayerSubProgram>
    {
        public override SerializedPlayerSubProgram Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new SerializedPlayerSubProgram();
            res.BlobIndex = reader.ReadUInt32();

            res.KeywordIndices = reader.ReadArray(r => r.ReadUInt16());
            reader.AlignStream();

            res.ShaderRequirements = reader.ReadInt64();
            res.GpuProgramType = (ShaderGpuProgramType)reader.ReadSByte();
            reader.AlignStream();
            return res;
        }
    }

}
