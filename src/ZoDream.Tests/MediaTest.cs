using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.Media;

namespace ZoDream.Tests
{
    [TestClass]
    public class MediaTest
    {
        // [TestMethod]
        public void TestFMOD()
        {
            var fileName = "1.fsb";
            FFmpegBinariesHelper.RegisterFFmpegBinaries();
            //AVFormatContext* ifmt_ctx = ffmpeg.avformat_alloc_context();
            //var res = ffmpeg.avformat_open_input(&ifmt_ctx, fileName, null, null);
            //ffmpeg.avformat_close_input(&ifmt_ctx);
            // Assert.AreEqual(res, 0);
        }
    }
}
