using System.Diagnostics;
using System.Text.RegularExpressions;
using ZoDream.BundleExtractor.Unity;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Drawing;
using ZoDream.Shared.Net;

namespace ZoDream.Tests
{
    [TestClass]
    public class PathTest
    {
        //[TestMethod]
        public void TestFinder()
        {
            var file = "apk\\Assets";
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

        [TestMethod]
        public void TestTake()
        {
            var items = new int[] { 2, 3, 6, 8, 9};
            Assert.IsTrue(items.Take(4).ToArray().Length == 4);
        }
        [TestMethod]
        public void TestColor()
        {
            var data = ColorConverter.SplitByte([15, 6], 0, out var i, 2, 1);
            Assert.IsTrue(data.Equal([3, 1]));
            Assert.AreEqual(i, 1);
        }
        [TestMethod]
        public void TestCombine()
        {
            var folder = NetService.Combine("D:\\a\\a\\d",
                new Uri("https://zodream.cn/d/b"),
                new Uri("https://zodream.cn/nav/c")
                );
            Assert.AreEqual(folder, "D:\\a\\a\\nav");
        }

    }
}