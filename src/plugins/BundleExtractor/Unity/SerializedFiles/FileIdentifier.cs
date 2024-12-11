﻿using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.SerializedFiles
{
    public class FileIdentifier
    {
        /// <summary>
        /// 2.1.0 and greater
        /// </summary>
        public static bool HasAssetPath(FormatVersion generation) => generation >= FormatVersion.Unknown_6;
        /// <summary>
        /// 1.2.0 and greater
        /// </summary>
        public static bool HasHash(FormatVersion generation) => generation >= FormatVersion.Unknown_5;

        //public bool IsFile(SerializedFile? file)
        //{
        //    return file is not null && file.NameFixed == PathName;
        //}

        public void Read(IBundleBinaryReader reader)
        {
            var version = reader.Get<FormatVersion>();
            if (HasAssetPath(version))
            {
                AssetPath = reader.ReadStringZeroTerm();
            }
            if (HasHash(version))
            {
                reader.Read(Guid, 0, Guid.Length);
                Type = (AssetType)reader.ReadInt32();
            }
            PathNameOrigin = reader.ReadStringZeroTerm();
            PathName = FileNameHelper.FixFileIdentifier(PathNameOrigin);
        }

        public string GetFilePath()
        {
            if (Type == AssetType.Meta)
            {
                return Guid.ToString();
            }
            return PathName;
        }

        public override string? ToString()
        {
            if (Type == AssetType.Meta)
            {
                return Guid.ToString();
            }
            return PathNameOrigin ?? base.ToString();
        }

        /// <summary>
        /// File path without such prefixes as archive:/directory/fileName
        /// </summary>
        public string PathName { get; set; }

        /// <summary>
        /// Virtual asset path. Used for cached files, otherwise it's empty.
        /// The file with that path usually doesn't exist, so it's probably an alias.
        /// </summary>
        public string AssetPath { get; set; }
        /// <summary>
        /// The type of the file
        /// </summary>
        public AssetType Type { get; set; }
        /// <summary>
        /// Actual file path. This path is relative to the path of the current file.
        /// The folder "library" often needs to be translated to "resources" in order to find the file on the file system.
        /// </summary>
        public string PathNameOrigin { get; set; }

        public byte[] Guid { get; set; } = new byte[16];
    }
}
