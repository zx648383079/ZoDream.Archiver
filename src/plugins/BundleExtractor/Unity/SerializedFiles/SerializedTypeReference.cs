using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.SerializedFiles
{
    public sealed class SerializedTypeReference : SerializedTypeBase
    {
        public string ClassName { get; set; } = "";
        public string Namespace { get; set; } = "";
        public string AsmName { get; set; } = "";

        public string FullName
        {
            get
            {
                return string.IsNullOrEmpty(Namespace)
                    ? ClassName
                    : $"{Namespace}.{ClassName}";
            }
        }

        protected override bool IgnoreScriptTypeForHash(FormatVersion formatVersion, Version unityVersion)
        {
            return false;
        }

        protected override void ReadTypeDependencies(IBundleBinaryReader reader)
        {
            ClassName = reader.ReadStringZeroTerm();
            Namespace = reader.ReadStringZeroTerm();
            AsmName = reader.ReadStringZeroTerm();
        }

    }
}
