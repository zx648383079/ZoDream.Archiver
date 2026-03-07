#pragma author zodream
#pragma description Klei Entertainment's KTEX texture format

#pragma endian big


struct Mipmap {
    s32 width;
    s32 height;
    s32 pitch;
    s32 length;
};

enum PlatformTypes: u16 {
    Default,
    PS3 = 10,
    Xbox360 = 12,
    PC = 12,
};

enum CompressionTypes: u16 {
    DXT1,
    DXT3,
    DXT5,
    RGBA = 4,
    RGB
};

enum TextureTypes: u16 {
    T_1D,
    T_2D,
    T_3D,
    CubeMapped
};

bitfield HeaderSpecs {
    PlatformTypes platform: 4;
    CompressionTypes compression: 5;
    TextureTypes texture_type: 4;
    mipmap_count: 5;
    flags: 2;
    fill: 12;
};

struct KtexFile {
    char signature[4] [[comment("KTEX")]];
    HeaderSpecs data;
    Mipmap items[data.mipmap_count];
};

KtexFile file @ 0x0;