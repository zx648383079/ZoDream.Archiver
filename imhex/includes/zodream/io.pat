#pragma author zodream
#pragma description Base Reader

import std.io;
import std.sys;
import type.leb128;

namespace auto zodream {
    fn fmt_collection(ref auto l) {
        return std::format("Size: {}", l.length);
    };

    struct List<T> {
        s32 length [[hidden]];
        if (length > 0) {
            T elements[length] [[inline]];
        }
    } [[format("fmt_collection")]];

    struct AlignTo<auto Alignment> {
        padding[Alignment- ((($ - 1) % Alignment) + 1)];
    } [[hidden]];

    struct AlignString {
        s32 length [[hidden]];
        char value[length] [[inline]];
        AlignTo<4>;
    } [[format("fmt_collection")]];

    struct Leb128String {
        type::LEB128 length [[hidden]];
        char value[length] [[inline]];
    } [[format("fmt_collection")]];

    struct Vector4 {
        float x;
        float y;
        float z;
        float w;
    };

    struct Quaternion {
        float x;
        float y;
        float z;
        float w;
    };

    struct Vector3 {
        float x;
        float y;
        float z;
    };

    struct Vector2 {
        float x;
        float y;
    };

    struct Rect {
        float x;
        float y;
        float width;
        float height;
    };

    struct Transform<T> {
        T t;
        Quaternion q;
        T s;
    };
    struct XForm<T> {
        T t;
        Quaternion q;
        T s;
    };

    struct Matrix2x2 {
        float[2] m1;
        float[2] m2;
    };
    struct Matrix3x3 {
        float[3] m1;
        float[3] m2;
        float[3] m3;
    };
    struct Matrix4x4 {
        float[4] m1;
        float[4] m2;
        float[4] m3;
        float[4] m4;
    };
};
