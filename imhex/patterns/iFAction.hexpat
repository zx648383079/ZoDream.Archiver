#pragma author zodream
#pragma description iFAction iFCon file

#pragma endian little


struct FileEntry {
    s32 offset;
    s32 length;
    s32 nameLength;
    char name[nameLength];
};


struct IFConFile {
    char signature[6] [[comment("iFFile")]];
    s32 indexSize;


    s32 fileCount;
    FileEntry files[fileCount];
};

IFConFile file @ 0x0;