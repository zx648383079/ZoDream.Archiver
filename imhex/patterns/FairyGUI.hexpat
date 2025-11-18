#pragma author zodream
#pragma description FairyGUI binary

#pragma endian big

import std.mem;
import std.core;


fn seek(u32 indexTablePos, u32 blockIndex) {
    u32 tmp = $;
    $ = indexTablePos;
    u8 segCount;
    if (blockIndex < segCount) {
        bool useShort;
        if (useShort) {
            $ = $ + 2 * blockIndex;
            u16 newPos;
        } else {
            $ = $ + 4 * blockIndex;
            u32 newPos;
        }

        if (newPos > 0) {
            $ = indexTablePos + newPos;
        } else {
            $ = tmp;
        }
    }
    else {
        $ = tmp;
    }
};

struct String {
    u16 length;
    char value[length];
};

struct Package {
    char signature[4] [[comment("FGUI")]];
    s32 version;
    bool is_compressed;
    String id;
    String name;
    padding[20];

    u32 next = $;
    seek(next, 4);
    s32 cnt;
    String stringTable[cnt];
};

Package file @ 0x0;