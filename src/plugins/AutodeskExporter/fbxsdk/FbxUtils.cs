using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace ZoDream.AutodeskExporter
{
    internal static class FbxUtils
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?FbxMalloc@fbxsdk@@YAPEAX_K@Z")]
        private static extern nint FbxMallocInternal(ulong size);

        [DllImport(NativeMethods.DllName, EntryPoint = "?FbxFree@fbxsdk@@YAXPEAX@Z")]
        private static extern void FbxFreeInternal(nint ptr);

        public static nint FbxMalloc(ulong size)
        {
            NativeMethods.Ready();
            //return Marshal.AllocHGlobal((int)size);
            return FbxMallocInternal(size);
        }

        public static void FbxFree(nint ptr)
        {
            // Marshal.FreeHGlobal(ptr);
            FbxFreeInternal(ptr);
        }

        public static unsafe string IntPtrToString(nint inPtr)
        {
            if (inPtr == IntPtr.Zero)
            {
                return string.Empty;
            }
            var ptr = (byte*)inPtr;
            var length = 0;
            while (ptr[length] != '\0')
            {
                length++;
            }
            var buffer = new byte[length];
            Marshal.Copy(inPtr, buffer, 0, length);
            return Encoding.ASCII.GetString(buffer);
        }

        public static unsafe string IntPtrToString(char* ptr, int length)
        {
            return new string(ptr, 0, length);
        }

        internal static EFbxType Convert<T>()
        {
            var type = typeof(T);
            if (type == typeof(double))
            {
                return EFbxType.eFbxDouble;
            }
            if (type == typeof(int))
            {
                return EFbxType.eFbxInt;
            }
            if (type == typeof(uint))
            {
                return EFbxType.eFbxUInt;
            }
            if (type == typeof(Enum))
            {
                return EFbxType.eFbxEnum;
            }
            if (type == typeof(Vector2) || type == typeof(FbxVector2) || type == typeof(FbxDouble2))
            {
                return EFbxType.eFbxDouble2;
            }
            if (type == typeof(Vector3) || type == typeof(FbxDouble3))
            {
                return EFbxType.eFbxDouble3;
            }
            if (type == typeof(Vector4) || type == typeof(FbxVector4) || type == typeof(FbxDouble4) || type == typeof(FbxColor))
            {
                return EFbxType.eFbxDouble4;
            }
            if (type == typeof(string) || type == typeof(FbxString))
            {
                return EFbxType.eFbxString;
            }
            throw new NotImplementedException();
        }
        internal static Type Convert(EFbxType type)
        {
            return type switch
            {
                EFbxType.eFbxChar or EFbxType.eFbxUChar => typeof(byte),
                EFbxType.eFbxShort => typeof(short),
                EFbxType.eFbxUShort => typeof(ushort),
                EFbxType.eFbxUInt => typeof(uint),
                EFbxType.eFbxLongLong => typeof(long),
                EFbxType.eFbxULongLong => typeof(ulong),
                EFbxType.eFbxBool => typeof(bool),
                EFbxType.eFbxInt or EFbxType.eFbxEnum => typeof(int),
                EFbxType.eFbxFloat => typeof(float),
                EFbxType.eFbxDouble => typeof(double),
                EFbxType.eFbxDouble2 => typeof(FbxDouble2),
                EFbxType.eFbxDouble3 => typeof(FbxDouble3),
                EFbxType.eFbxDouble4 => typeof(FbxDouble4),
                EFbxType.eFbxDouble4x4 => typeof(FbxMatrix),
                EFbxType.eFbxString => typeof(FbxString),
                EFbxType.eFbxTime => typeof(FbxTime),
                _ => throw new NotImplementedException(),
            };
        }

        internal static ulong SizeOf(EFbxType type)
        {
            return type switch
            {
                EFbxType.eFbxChar or EFbxType.eFbxUChar => 1,
                EFbxType.eFbxShort => sizeof(short),
                EFbxType.eFbxUShort => sizeof(ushort),
                EFbxType.eFbxUInt => sizeof(uint),
                EFbxType.eFbxLongLong => sizeof(long),
                EFbxType.eFbxULongLong => sizeof(ulong),
                EFbxType.eFbxBool => sizeof(bool),
                EFbxType.eFbxInt or EFbxType.eFbxEnum => sizeof(int),
                EFbxType.eFbxFloat => sizeof(float),
                EFbxType.eFbxDouble => sizeof(double),
                EFbxType.eFbxDouble2 => FbxDouble2.SizeOfThis,
                EFbxType.eFbxDouble3 => FbxDouble3.SizeOfThis,
                EFbxType.eFbxDouble4 => FbxDouble4.SizeOfThis,
                EFbxType.eFbxDouble4x4 => FbxMatrix.SizeOfThis,
                EFbxType.eFbxString => FbxString.SizeOfThis,
                EFbxType.eFbxTime => FbxTime.SizeOfThis,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
