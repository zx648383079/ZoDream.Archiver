using ZoDream.Shared;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.BundleFiles
{
    public abstract class BundleHeader : IBundleHeader
    {
        protected abstract string MagicString { get; }
        public UnityBundleVersion Version { get; set; }
        /// <summary>
        /// Generation version
        /// </summary>
        public string? UnityWebBundleVersion { get; set; }
        /// <summary>
        /// Actual engine version
        /// </summary>
        public string? UnityWebMinimumRevision { get; set; }
        public virtual void Read(IBundleBinaryReader reader)
        {
            var signature = reader.ReadStringZeroTerm();
            Expectation.ThrowIfNotSignature(MagicString, signature);
            Version = (UnityBundleVersion)reader.ReadInt32();
            Expectation.ThrowIfNotVersion(Version >= 0);
            UnityWebBundleVersion = reader.ReadStringZeroTerm();
            UnityWebMinimumRevision = reader.ReadStringZeroTerm();
        }
    }
}
