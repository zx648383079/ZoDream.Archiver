using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    public partial class OtherBundleElementScanner(ICommandArguments package, IBundleOptions options) : IBundleParser
    {

        public bool IsCounterSide => package.Contains("cs");
        public bool IsSchoolGirlStrikers => package.Contains("sgs");
        public bool IsPartyAnimals => package.Contains("pa");
        public bool IsMuvLuvDimensions => package.Contains("mld");
        public bool IsJJKPhantomParade => package.Contains("jjkpp");
        public bool IsGirlsFrontline => package.Contains("gf");
        public bool IsCodenameJump => package.Contains("cj");
        public bool IsProjectSekai => package.Contains("ps");
        public bool IsAliceGearAegis => package.Contains("aga");
        public bool IsImaginaryFest => package.Contains("if");
        public bool IsDreamscapeAlbireo => package.Contains("da");
        public bool IsAnchorPanic => package.Contains("ap");
        public bool IsHelixWaltz2 => package.Contains("hw2");
        public bool IsFantasyOfWind => package.Contains("fow");
        public bool IsEnsembleStars => package.Contains("es");
        public bool IsAcmeis => package.Contains("my");
        public bool IsFakeHeader => package.Contains("fake");
        public bool IsXorHeader => package.Contains("xor");


        public IBundleBinaryReader OpenRead(string fullPath)
        {
            return Parse(File.OpenRead(fullPath), new FilePath(fullPath));
        }

        public IBundleBinaryReader Parse(Stream input, IFilePath sourcePath)
        {
            if (IsFakeHeader && !FileNameHelper.IsCommonFile(sourcePath.Name))
            {
                input = ParseFakeHeader(input);
            }
            if (IsXorHeader)
            {
                input = ParseXor(input);
            }
            return new BundleBinaryReader(input, EndianType.BigEndian);
        }
    }
}
