#pragma author zodream
#pragma description DBreeze

#pragma endian big



struct EntryItem {
    
};

struct DBreezeFile {
    padding[12]
    s24 count;
    $ = 64;
    EntryItem items[count];
};

DBreezeFile doc @ 0x0;