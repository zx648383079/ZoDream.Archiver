using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class MiHoYoElementScanner(string package) : IBundleStorage
    {
        /// <summary>
        /// 原神
        /// </summary>
        public bool IsGI => package.Contains("genshin");

        internal bool IsGICB1 => IsGI;

        internal bool IsGIPack => IsGI;

        internal bool IsGICB2 => IsGI;

        internal bool IsGICB3 => IsGI;

        internal bool IsGICB3Pre => IsGI;

        internal bool IsGISubGroup => IsGI || IsGICB2 || IsGICB3 || IsGICB3Pre;

        internal bool IsGIGroup => IsGI || IsGIPack || IsGICB1 || IsGICB2 || IsGICB3
                || IsGICB3Pre;
        /// <summary>
        /// 铁道
        /// </summary>
        internal bool IsSR => package.Contains("hkrpg");

        internal bool IsSRCB2 => IsSR;

        internal bool IsSRGroup => IsSRCB2 || IsSR;
        /// <summary>
        /// 崩坏3
        /// </summary>
        internal bool IsBH3 => package.Contains("bh3");

        internal bool IsBH3Group => IsBH3;
        /// <summary>
        /// 绝区零
        /// </summary>
        internal bool IsZZZCB1 => package.Contains("zzz");

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
            return new BundleBinaryReader(input, EndianType.BigEndian);
        }

        private static bool CheckHeader(IBundleBinaryReader reader, int offset)
        {
            short value = 0;
            var pos = reader.Position;
            while (value != -1 && reader.Position <= pos + offset)
            {
                value = reader.ReadInt16();
            }
            var isNewHeader = reader.Position - pos == offset;
            reader.Position = pos;
            return isNewHeader;
        }
    }
}
