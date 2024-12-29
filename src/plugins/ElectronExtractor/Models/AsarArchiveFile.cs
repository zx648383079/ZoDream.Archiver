namespace ZoDream.ElectronExtractor.Models
{
    internal class AsarArchiveFile
    {
        public long Offset { get; set; }

        public long Size { get; set; }

        public bool Unpacked { get; set; }

        public bool Executable { get; private set; }

        public bool Link { get; private set; }

        public AsarArchiveFileIntegrity? Integrity { get; private set; }

        public AsarArchiveFileCollection? Files { get; internal set; }
    }
}
