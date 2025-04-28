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
    } [[format("zodream::fmt_collection")]];

    struct AlignTo<auto Alignment> {
        padding[Alignment- ((($ - 1) % Alignment) + 1)];
    } [[hidden]];

    struct AlignString {
        s32 length;
        char value[length];
        AlignTo<4>;
    };

    struct Leb128String {
        type::LEB128 length;
        char value[length];
    };

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
        float m1[2];
        float m2[2];
    };
    struct Matrix3x3 {
        float m1[3];
        float m2[3];
        float m3[3];
    };
    struct Matrix4x4 {
        float m1[4];
        float m2[4];
        float m3[4];
        float m4[4];
    };
}