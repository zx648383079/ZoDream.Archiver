﻿using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxImporter : FbxIOBase
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxImporter@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern IntPtr CreateFromManager(IntPtr pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Initialize@FbxImporter@fbxsdk@@UEAA_NPEBDHPEAVFbxIOSettings@2@@Z")]
        private static extern bool InitializeInternal(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string pFilename, int pFileFormat, IntPtr pIOSettings);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetFileVersion@FbxImporter@fbxsdk@@QEAAXAEAH00@Z")]
        private static extern void GetFileVersionInternal(IntPtr handle, out int pMajor, out int pMinor, out int pRevision);

        [DllImport(NativeMethods.DllName, EntryPoint = "?IsFBX@FbxImporter@fbxsdk@@QEAA_NXZ")]
        private static extern bool IsFBXInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Import@FbxImporter@fbxsdk@@QEAA_NPEAVFbxDocument@2@_N@Z")]
        private static extern bool ImportInternal(IntPtr handle, IntPtr pDocument, bool pNonBlocking);

        public FbxImporter(FbxManager Manager, string Name)
            : base(CreateFromManager(Manager.Handle, Name))
        {
        }

        public bool Initialize(string filename, int fileFormat = -1, FbxIOSettings? ioSettings = null)
        {
            return InitializeInternal(Handle, filename, fileFormat, ioSettings is null ? IntPtr.Zero : ioSettings.Handle);
        }

        public void GetFileVersion(out int major, out int minor, out int revision)
        {
            GetFileVersionInternal(Handle, out major, out minor, out revision);
        }

        public bool IsFBX()
        {
            return IsFBXInternal(Handle);
        }

        public bool Import(FbxDocument document, bool nonBlocking = false)
        {
            return ImportInternal(Handle, document.Handle, nonBlocking);
        }
    }

}
