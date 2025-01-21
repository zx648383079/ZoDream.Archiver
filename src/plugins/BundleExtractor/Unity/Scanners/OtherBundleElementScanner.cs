using System.IO;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class OtherBundleElementScanner(string package, IBundleOptions options) : 
        IBundleElementScanner, IBundleStorage
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
            if (IsFakeHeader && !FileNameHelper.IsCommonFile(fileName))
            {
                input = ParseFakeHeader(input);
            }
            return new BundleBinaryReader(input, EndianType.BigEndian);
        }

        public bool TryRead(IBundleBinaryReader reader, object instance)
        {
            if (reader.TryGet<UnityVersion>(out var version) && 
                version.Type == UnityVersionType.China)
            {
                return new TuanJieElementScanner(package, options).TryRead(reader, instance);
            }
            if (instance is IElementLoader l)
            {
                l.Read(reader);
                return true;
            }
            return false;
        }
    }
}
