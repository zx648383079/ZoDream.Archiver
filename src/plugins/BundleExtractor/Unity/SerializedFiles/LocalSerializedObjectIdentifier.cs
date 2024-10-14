using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.SerializedFiles
{
    public struct LocalSerializedObjectIdentifier
    {
        public void Read(EndianReader reader, FormatVersion version)
        {
            LocalSerializedFileIndex = reader.ReadInt32();
            if (ObjectInfo.IsLongID(version))
            {
                reader.AlignStream();
                LocalIdentifierInFile = reader.ReadInt64();
            }
            else
            {
                LocalIdentifierInFile = reader.ReadInt32();
            }
        }

        public override string ToString()
        {
            return $"[{LocalSerializedFileIndex}, {LocalIdentifierInFile}]";
        }

        public int LocalSerializedFileIndex { get; set; }
        public long LocalIdentifierInFile { get; set; }
    }
}
