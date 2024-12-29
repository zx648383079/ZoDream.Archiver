namespace ZoDream.ElectronExtractor.Models
{
    internal class AsarArchiveFileIntegrity
    {
        private const string InternalAlgorithmName = "SHA256";
        private const int InternalBlockSize = 4 * 1024 * 1024;
        public int BlockSize { get; set; }

        public string Algorithm { get; set; }

        public string Hash { get; set; }

        public string[] Blocks { get; private set; }
    }
}
