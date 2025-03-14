﻿using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxDocument : FbxCollection
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?GetDocumentInfo@FbxDocument@fbxsdk@@QEBAPEAVFbxDocumentInfo@2@XZ")]
        protected static extern IntPtr GetDocumentInfoInternal(IntPtr handle);

        public FbxDocument() { }
        public FbxDocument(IntPtr InHandle)
            : base(InHandle)
        {
        }
    }

}
