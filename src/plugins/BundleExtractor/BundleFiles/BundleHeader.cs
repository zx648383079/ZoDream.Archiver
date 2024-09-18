﻿using System.Diagnostics;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.BundleFiles
{
    public abstract class BundleHeader : IBundleHeader
    {
        protected abstract string MagicString { get; }
        public BundleVersion Version { get; set; }
        /// <summary>
        /// Generation version
        /// </summary>
        public string? UnityWebBundleVersion { get; set; }
        /// <summary>
        /// Actual engine version
        /// </summary>
        public string? UnityWebMinimumRevision { get; set; }
        public virtual void Read(EndianReader reader)
        {
            var signature = reader.ReadStringZeroTerm();
            Debug.Assert(signature == MagicString);
            Version = (BundleVersion)reader.ReadInt32();
            Debug.Assert(Version >= 0);
            UnityWebBundleVersion = reader.ReadStringZeroTerm();
            UnityWebMinimumRevision = reader.ReadStringZeroTerm();
        }
    }
}
