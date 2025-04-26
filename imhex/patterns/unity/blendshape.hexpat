#pragma author zodream
#pragma description Unity Blend Shape

import zodream.io;
import type.leb128;
import std.mem;
import std.sys;

fn nextIsEnd(u32 address) {
    if (std::mem::read_unsigned(address, 1) != 0x12) {
        return true;
    }
    u8 i = 1;
    while(std::mem::read_unsigned(address + i, 1) >= 0x80) {
        i = i + 1;
    }
    return std::mem::read_unsigned(address + i + 1, 1) == 0xA;
};

fn isFloatTag(u32 address) {
    u8 tag @ address;
    return tag == 0x1D || tag == 0x25 || tag == 0x2D;
};


struct BlendShapeFloat {
    u8 beginTag;
    float value;
};// [[format("{}", value)]];

struct BlendShapePoint {
    u8 beginTag;
    std::assert(beginTag == 0x12, "Invalid tag!");
    type::LEB128 dataSize;
    u32 start = $;
    u8 dataSignuature;
    type::LEB128 index;
    BlendShapeFloat data[while(isFloatTag($))];
    std::assert($ - start == dataSize, "Invalid data!");
};


struct BlendShapePartItem {
    zodream::Leb128String name;
    u8 splitTag;
    std::assert(splitTag == 0x12, "Invalid tag!");
    type::LEB128 dataSize;
    u8 dataSignuature;
    if (dataSignuature == 0xD) {
        char bodySignuature[4] [[comment("0x0 0x0 0xC8 0x42")]];
        BlendShapePoint pointItems[while(!nextIsEnd($))];
    } else {
        BlendShapePartItem child;
    }
};

struct BlendShapePart {
    char partSignuature [[comment("0xA | 0x12")]];
    type::LEB128 dataSize;
    char dataSignuature [[comment("0xA")]];
    std::assert(dataSignuature == 0xA, "Invalid tag!");
    BlendShapePartItem body;
};

struct BlendShape {
    char signuature [[comment("0xA")]];
    std::assert(signuature == 0xA, "Invalid tag!");
    type::LEB128 dataSize;
    char dataSignuature [[comment("0xA")]];
    std::assert(dataSignuature == 0xA, "Invalid tag!");
    zodream::Leb128String name;
    BlendShapePart partItems[while(!std::mem::eof() && std::mem::read_unsigned($, 1) != 0x18)];
    char endSignuature[4] [[comment("0x18 0xC0, 0xE3, 0x1")]];
};

BlendShape shape @ 0x0;