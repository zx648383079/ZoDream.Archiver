using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.BundleExtractor.Unity.BundleFiles;
using ZoDream.Shared.Compression.Own;
using ZoDream.Shared.IO;

namespace ZoDream.Tests
{
    [TestClass]
    public class StreamTest
    {
        [TestMethod]
        public void TestByte()
        {
            var count = 31;
            var index = 7;
            var b = OwnDictionaryWriter.MergeByte(index, count);
            var (c, d) = OwnDictionaryWriter.SplitByte(b);
            Assert.AreEqual(c, index);
            Assert.AreEqual(d, count);
        }

        //[TestMethod]
        public void TestFinder()
        {
            var finder = new StreamFinder("UnityFS")
            {
                IsMatchFirst = true,
            };
            using var fs = File.OpenRead(".bundle");
            var res = finder.MatchFile(fs);
            Assert.IsTrue(res);
        }
    }
}
