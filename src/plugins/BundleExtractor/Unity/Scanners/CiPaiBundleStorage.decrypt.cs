using System;
using System.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class CiPaiBundleElementScanner
    {
        /// <summary>
        /// 过时的
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Obsolete(">= 2.0.0", true)]
        private Stream DecryptPerpetualNovelty(Stream input)
        {
            var output = new MemoryStream();
            var buffer = new byte[120];
            input.ReadExactly(buffer);
            var key = buffer[51];
            for (var i = 50; i < 120; i++)
            {
                buffer[i] ^= key;
            }
            output.Write(buffer);
            input.CopyTo(output);
            output.Position = 0;
            input.Dispose();
            return output;
        }
    }
}
