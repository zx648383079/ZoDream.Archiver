using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.SerializedFiles
{
    public class SerializedFileHeader
    {
        /// <summary>
		/// Size of the metadata parts of the file
		/// </summary>
		public long MetadataSize { get; set; }
        /// <summary>
        /// Size of the whole file
        /// </summary>
        public long FileSize { get; set; }
        /// <summary>
        /// File format version. The number is required for backward compatibility and is normally incremented after the file format has been changed in a major update
        /// </summary>
        public FormatVersion Version { get; set; }
        /// <summary>
        /// Offset to the serialized object data. It starts at the data for the first object
        /// </summary>
        public long DataOffset { get; set; }
        /// <summary>
        /// Presumably controls the byte order of the data structure. This field is normally set to 0, which may indicate a little endian byte order.
        /// </summary>
        public bool Endianess { get; set; }

        public const int HeaderMinSize = 16;

        public const int MetadataMinSize = 13;


        /// <summary>
        /// 3.5.0 and greater / Format Version 9 +
        /// </summary>
        public static bool HasEndianess(FormatVersion generation) => generation >= FormatVersion.Unknown_9;

        /// <summary>
        /// 2020.1.0 and greater / Format Version 22 +
        /// </summary>
        public static bool HasLargeFilesSupport(FormatVersion generation) => generation >= FormatVersion.LargeFilesSupport;
        public void Read(EndianReader reader)
        {
            //For gen 22+ these will be zero
            MetadataSize = reader.ReadInt32();
            FileSize = reader.ReadUInt32();

            //Read generation
            Version = (FormatVersion)reader.ReadInt32();

            //For gen 22+ this will be zero
            DataOffset = reader.ReadUInt32();

            if (HasEndianess(Version))
            {
                Endianess = reader.ReadBoolean();
                reader.AlignStream();
            }
            if (HasLargeFilesSupport(Version))
            {
                MetadataSize = reader.ReadUInt32();
                FileSize = reader.ReadInt64();
                DataOffset = reader.ReadInt64();
                reader.ReadInt64(); // unknown
            }

            if (MetadataSize <= 0)
            {
                throw new Exception($"Invalid metadata size {MetadataSize}");
            }

            if (!Enum.IsDefined(typeof(FormatVersion), Version))
            {
                throw new Exception($"Unsupported file generation {Version}'");
            }
        }



    }
}
