using ZoDream.SpineExporter;

namespace ZoDream.Tests
{
    [TestClass]
    public class JsonTest
    {

        //[TestMethod]
        public void TestSpine()
        {
            var fileName = "";
            // using var fs = File.OpenRead(fileName);
            var data = new SkeletonJsonReader().Deserialize(File.ReadAllText(fileName), fileName);
            Assert.IsTrue(data is not null);
        }
    }
}
