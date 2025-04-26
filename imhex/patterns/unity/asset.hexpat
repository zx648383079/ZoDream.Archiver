#pragma author zodream
#pragma description Unity Asset

u64 beginPos in;

enum ArchiveFlags: u32 {
    CompressionTypeMask = 0x3f,
    BlocksAndDirectoryInfoCombined = 0x40,
    BlocksInfoAtTheEnd = 0x80,
    OldWebPluginCompatibility = 0x100,
    BlockInfoNeedPaddingAtStart = 0x200
};

struct FileHeader {
    char signature[20] [[comment("UnityFS")]];
    u32 version;
    char unityVersion;
    char unityRevision;
    s64 size;
    u32 compressedBlocksInfoSize;
    ArchiveFlags flags;
    if (signature == "UnityFS") {
        $ = $ + 1;
    }
    if (version >= 7) {
        if ($ % 16 > 0) {
            $ = $ + 16 - $ % 16;
        }
    }
};

