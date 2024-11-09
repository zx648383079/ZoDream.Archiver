using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.Compression.Own;

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
    }
}
