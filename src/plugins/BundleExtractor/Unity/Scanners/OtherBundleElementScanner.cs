using System.IO;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class OtherBundleElementScanner(string package) : IBundleElementScanner, IBundleStorage
    {

        public bool IsCounterSide => package.Contains("");
        public bool IsSchoolGirlStrikers => package.Contains("");
        public bool IsPartyAnimals => package.Contains("");
        public bool IsMuvLuvDimensions => package.Contains("");
        public bool IsJJKPhantomParade => package.Contains("");
        public bool IsReverse1999 => package.Contains("");
        public bool IsGirlsFrontline => package.Contains("");
        public bool IsCodenameJump => package.Contains("");
        public bool IsProjectSekai => package.Contains("");
        public bool IsAliceGearAegis => package.Contains("");
        public bool IsImaginaryFest => package.Contains("");
        public bool IsDreamscapeAlbireo => package.Contains("");
        public bool IsAnchorPanic => package.Contains("");
        public bool IsHelixWaltz2 => package.Contains("");
        public bool IsFantasyOfWind => package.Contains("");
        public bool IsEnsembleStars => package.Contains("");

        public Stream Open(string path)
        {
            return File.OpenRead(path);
        }

        public bool TryRead(IBundleBinaryReader reader, object instance)
        {
            if (instance is IElementLoader l)
            {
                l.Read(reader);
                return true;
            }
            return false;
        }
    }
}
