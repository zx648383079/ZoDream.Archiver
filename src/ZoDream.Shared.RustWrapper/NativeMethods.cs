using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ZoDream.Shared.RustWrapper
{
    internal unsafe static partial class NativeMethods
    {
        static NativeMethods()
        {
            NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, DllImportResolver);
        }

        static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName == RustDllName)
            {
                string? extension;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    extension = ".dll";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    extension = ".dylib";
                }
                else
                {
                    extension = ".so";
                }
                var entry = Path.Combine(AppContext.BaseDirectory, Enum.GetName(RuntimeInformation.ProcessArchitecture).ToLower(), RustDllName + extension);
                return NativeLibrary.Load(entry, assembly, searchPath);
            }

            return IntPtr.Zero;
        }
    }
}
