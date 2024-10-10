using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace ZoDream.Tests
{
    [TestClass]
    public class PathTest
    {
        [TestMethod]
        public void TestFinder()
        {
            var file = "D:\\zodream\\apk\\nn4\\Assets";
            var items = Directory.GetDirectories(file, "Assets", SearchOption.AllDirectories);
            Assert.IsNotNull(items);
        }

        [TestMethod]
        public void TestMatch()
        {
            var name = "r.rar.004";
            var match = Regex.Match(name, @"(\.part\d+\.(rar|zip|7z)|\.(zip|7z|rar)\.\d+|\.(z|r)\d+|\.(zip))$");
            Assert.IsTrue(match.Success);
        }

        [TestMethod]
        public void TestSize()
        {
            var text = "4.5MB";
            var match = Regex.Match(text, @"([\d\.]+)\s*([PTGMKB]?)");
            Assert.IsTrue(match.Success);
        }
    }
}