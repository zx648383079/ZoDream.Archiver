using System.Text;
using ZoDream.AutodeskExporter;
using ZoDream.BundleExtractor.Cocos;
using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.KhronosExporter;
using ZoDream.Shared.Bundle;
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

        [TestMethod]
        public void TestXOR()
        {
            byte[] buffer = [0x93, 0xa8, 0xaf, 0xb2];
            var target = "UnityFS"u8;
            var format = XORStream.Recognize(buffer, target);
            Assert.IsEmpty(format);
        }

        [TestMethod]
        public void TestCached()
        {
            var str = "测试测hi克服asdasd的困难开发那是肯定asdasdas能进卡空间的asdasdasdasdasd妇女健asdasd康四点八九可能比较空洞看到你可能是看得见";
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(str));
            var input = new CachedStream(ms, 16);
            var data = input.ToArray();
            Assert.AreEqual(Encoding.UTF8.GetString(data), str);
        }
        [TestMethod]
        public void TestLength()
        {
            var buffer = new byte[20];
            var max = 1000;//Math.Pow(1024, 3);
            for (int i = 251; i < max; i++)
            {
                var c = OwnHelper.WriteLength(i, buffer, 0);
                var res = OwnHelper.ReadLength(buffer, 0, out var rc);
                Assert.AreEqual(i, (int)res);
                Assert.AreEqual(c, rc);
            }
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

        //[TestMethod]
        public void TestGltf()
        {
            var fileName = "F:\\Desktop\\bad.glb";
            using var fs = File.OpenRead(fileName);
            var res = new GlbReader().Read(fs);
            if (res is not null)
            {
                res.FileName = fileName;
                var items = res.Accessors.Select(res.ReadAccessorBuffer).ToArray();
            }
            Assert.IsTrue(res is not null);
        }

        //[TestMethod]
        public void TestFbx()
        {
            var data = FbxContext.ToEuler(new(0.00392036f, -0.00511095f, -0.613622f, 0.789573f));
            var fileName = "F:\\Desktop\\bad.fbx";
            var res = new FbxReader(fileName).Read();
            Assert.IsTrue(res);
        }

        //[TestMethod]
        public void TestSpv()
        {
            var fileName = "F:\\apk\\zmxs\\mesh.bin";
            using var fs = File.OpenRead(fileName);
            Assert.IsTrue(fs is not null);
        }



        //[TestMethod]
        public void TestBc()
        {
            var src = "F:\\apk\\test\\check_version_view.lua";
            new BlowfishReader(BundleChunk.CreateFrom(src))
                .ExtractTo("F:\\apk\\test_output", Shared.Models.ArchiveExtractMode.Overwrite);
            Assert.IsTrue(true);
        }
    }
}
