using ZoDream.BundleExtractor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.SerializedFiles
{
    public sealed class SerializedType : SerializedTypeBase
    {
        public int[] TypeDependencies { get; set; } = [];

        protected override bool IgnoreScriptTypeForHash(FormatVersion formatVersion, UnityVersion unityVersion)
        {
            //This code is most likely correct, but not guaranteed.
            //Reverse engineering it was painful, and it's possible that mistakes were made.
            return !unityVersion.Equals(0, 0, 0) && unityVersion < WriteIDHashForScriptTypeVersion;
        }

        protected override void ReadTypeDependencies(EndianReader reader)
        {
            TypeDependencies = reader.ReadInt32Array();
        }


        private static UnityVersion WriteIDHashForScriptTypeVersion { get; } = new UnityVersion(2018, 3, 0, UnityVersionType.Alpha, 1);
    }
}
