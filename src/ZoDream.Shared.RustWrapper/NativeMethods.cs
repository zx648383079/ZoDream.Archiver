﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
                var extension = "";

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
                    entry = Path.Combine(AppContext.BaseDirectory, RustDllName + extension);
                }
                return NativeLibrary.Load(entry, assembly, searchPath);
            }

            return IntPtr.Zero;
        }
    }
}
