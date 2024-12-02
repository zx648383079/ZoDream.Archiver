using System.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class PaperElementScanner
    {
        
        private Stream DecryptLoveAndDeepSpace(Stream input)
        {
            return new LoveAndDeepSpaceStream(input);
        }
    }
}
