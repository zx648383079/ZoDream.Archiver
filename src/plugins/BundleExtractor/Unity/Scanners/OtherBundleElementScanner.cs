using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

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
        public bool IsFakeHeader => package.Contains("fake");

        public Stream Open(string fullPath)
        {
            return File.OpenRead(fullPath);
        }

        public IBundleBinaryReader OpenRead(string fullPath)
        {
            return OpenRead(Open(fullPath), fullPath);
        }

        public IBundleBinaryReader OpenRead(Stream input, string fileName)
        {
            if (IsFakeHeader && !FilenameHelper.IsCommonFile(fileName))
            {
                input = ParseFakeHeader(input);
            }
            return new BundleBinaryReader(input, EndianType.BigEndian);
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
