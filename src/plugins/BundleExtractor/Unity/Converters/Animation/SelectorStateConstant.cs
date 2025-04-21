using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SelectorStateConstantConverter : BundleConverter<SelectorStateConstant>
    {
        public override SelectorStateConstant? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new SelectorStateConstant
            {
                TransitionConstantArray = reader.ReadArray<SelectorTransitionConstant>(serializer),


                FullPathID = reader.ReadUInt32(),
                isEntry = reader.ReadBoolean()
            };
            reader.AlignStream();
            return res;
        }
    }

}
