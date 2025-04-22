using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.SerializedFiles
{
    public sealed class SerializedType : SerializedTypeBase
    {
        public int[] TypeDependencies { get; set; } = [];

        protected override bool IgnoreScriptTypeForHash(FormatVersion formatVersion, Version unityVersion)
        {
            //This code is most likely correct, but not guaranteed.
            //Reverse engineering it was painful, and it's possible that mistakes were made.
            return !unityVersion.Equals(0, 0, 0) && unityVersion < WriteIDHashForScriptTypeVersion;
        }

        protected override void ReadTypeDependencies(IBundleBinaryReader reader)
        {
            TypeDependencies = reader.ReadInt32Array();
        }


        private static Version WriteIDHashForScriptTypeVersion { get; } = new(2018, 3, 0, VersionType.Alpha, 1);
    }
}
