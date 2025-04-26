#pragma author zodream
#pragma description Unity Vertex Map

import zodream.io;
import type.leb128;
import std.mem;
import std.sys;


struct VertexMapFloat {
    u8 beginTag;
    std::assert(beginTag == 0x1A, "Invalid tag!");
    type::LEB128 len;
    u32 start = $;
    // u8 value[len];
    type::LEB128 data[while(!std::mem::eof() && $ - start < len)];
    std::assert($ - start == len, "Invalid data!");
};// [[format("{:f}", value)]];


struct VertexMapPart {
    char partSignature [[comment("0xA | 0x12")]];
    zodream::Leb128String name;
};

struct VertexMap {
    char partSignature [[comment("0xA")]];
    std::assert(partSignature == 0xA, "Invalid tag!");
    type::LEB128 dataSize;
    VertexMapPart nameItems[while(std::mem::read_unsigned($, 1) != 0x1A)];
    VertexMapFloat pointItems[while(!std::mem::eof() && std::mem::read_unsigned($, 1) == 0x1A)];
};

VertexMap vertex @ 0x0;