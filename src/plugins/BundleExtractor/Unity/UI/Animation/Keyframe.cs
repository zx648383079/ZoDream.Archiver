using System;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Keyframe<T>
    {
        public float time;
        public T value;
        public T inSlope;
        public T outSlope;
        public int weightedMode;
        public T inWeight;
        public T outWeight;

        public Keyframe(float time, T value, T inSlope, T outSlope, T weight)
        {
            this.time = time;
            this.value = value;
            this.inSlope = inSlope;
            this.outSlope = outSlope;
            weightedMode = 0;
            inWeight = weight;
            outWeight = weight;
        }

        public Keyframe(IBundleBinaryReader reader, Func<T> readerFunc)
        {
            time = reader.ReadSingle();
            value = readerFunc();
            inSlope = readerFunc();
            outSlope = readerFunc();
            if (reader.Get<UnityVersion>().Major >= 2018) //2018 and up
            {
                weightedMode = reader.ReadInt32();
                inWeight = readerFunc();
                outWeight = readerFunc();
            }
        }

        private int ToSerializedVersion(UnityVersion version)
        {
            if (version.GreaterThanOrEquals(2018)) //2018 and up
            {
                return 3;
            }
            else if (version.GreaterThanOrEquals(5, 5))
            {
                return 2;
            }
            return 1;
        }
    }

}
