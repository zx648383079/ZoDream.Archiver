namespace ZoDream.BundleExtractor.Unity.BundleFiles
{
    public class WebBundleHeader : RawWebBundleHeader
    {
        internal const string UnityWebMagic = "UnityWeb";
        protected override string MagicString => UnityWebMagic;
    }
}
