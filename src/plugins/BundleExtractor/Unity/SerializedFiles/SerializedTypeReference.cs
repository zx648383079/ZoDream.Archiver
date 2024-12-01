using ZoDream.BundleExtractor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.IO;
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

        protected override bool IgnoreScriptTypeForHash(FormatVersion formatVersion, UnityVersion unityVersion)
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
