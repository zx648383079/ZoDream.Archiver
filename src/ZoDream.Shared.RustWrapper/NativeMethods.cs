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
                var path = "runtimes/";
                string? extension;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    path += "win-";
                    extension = ".dll";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    path += "osx-";
                    extension = ".dylib";
                }
                else
                {
                    path += "linux-";
                    extension = ".so";
                }

                if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
                {
                    path += "x86";
                }
                else if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                {
                    path += "x64";
                }
                else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                {
                    path += "arm64";
                }

                path += "/native";
                var entry = Path.Combine(AppContext.BaseDirectory, path, RustDllName + extension);
                if (!File.Exists(entry))
                {
#if DEBUG
                    entry = Path.Combine(AppContext.BaseDirectory, "../../../../zodream/target/debug", RustDllName + extension);
#else
                    entry = Path.Combine(AppContext.BaseDirectory, RustDllName + extension);
#endif
                }
                return NativeLibrary.Load(entry, assembly, searchPath);
            }

            return IntPtr.Zero;
        }
    }
}
