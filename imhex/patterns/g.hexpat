#pragma author zodream
#pragma description Eastward


import std::string;

using NullString = std::string::NullStringBase;


struct EntryItem {
    NullString name;
    s32 offset;
    s32 compressType;
    bool isCompressed = compressType == 2;
    s32 decompressedSize;
    s32 compressedSize;
};

struct GFile {
    char magic[4] [comment("7j\0\0")]
    s32 count;
    EntryItem items[count];
};

GFile doc @ 0x0;