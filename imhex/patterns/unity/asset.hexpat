#pragma author zodream
#pragma description Unity Asset

#pragma endian big

u64 beginPos in;

import std.string;
import zodream.io;

enum ArchiveFlags: u32 {
    None = 0,

    CompressionBit1 = 0x1,
    CompressionBit2 = 0x2,
    CompressionBit3 = 0x4,
    CompressionBit4 = 0x8,
    CompressionBit5 = 0x10,
    CompressionBit6 = 0x20,
    CompressionTypeMask = 0x3f,
    BlocksAndDirectoryInfoCombined = 0x40,
    BlocksInfoAtTheEnd = 0x80,
    OldWebPluginCompatibility = 0x100,
    BlockInfoNeedPaddingAtStart = 0x200
};

struct FileHeader {
    char signature[] [[comment("UnityFS")]];
    u32 version;
    char unityVersion[];
    char unityRevision[];
    s64 fileSize;
    u32 compressedBlocksInfoSize;
    ArchiveFlags flags;
    if (signature == "UnityFS") {
        padding[1];
    }
    if (version >= 7) {
        zodream::AlignTo<4>;
    }
};

struct FileStreamBundle {
    FileHeader header;
};

FileStreamBundle bunlde @ 0x0;