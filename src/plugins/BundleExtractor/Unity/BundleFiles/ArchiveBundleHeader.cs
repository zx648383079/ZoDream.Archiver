namespace ZoDream.BundleExtractor.Unity.BundleFiles
{
    public class ArchiveBundleHeader : BundleHeader
    {
        internal const string UnityArchiveMagic = "UnityArchive";
        protected override string MagicString => UnityArchiveMagic;
    }
}
