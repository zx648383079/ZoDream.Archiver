using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.FModExporter;
using ZoDream.Shared.Media;

namespace ZoDream.Tests
{
    [TestClass]
    public class MediaTest
    {
         //[TestMethod]
        public void TestFMOD()
        {
            var fileName = "";
            using var fs = File.OpenRead(fileName);
            //using var os = File.OpenWrite(fileName + ".wav");
            new FModExporter.RiffReader(fs, null)
                .ExtractToDirectory("D:\\Desktop\\h", Shared.Models.ArchiveExtractMode.Overwrite);
            
            Assert.IsTrue(true);
        }
    }
}
