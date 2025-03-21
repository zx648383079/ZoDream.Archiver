﻿using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxBlendShapeChannel : FbxObject
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxBlendShapeChannel@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern nint CreateFromObject(nint pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?AddTargetShape@FbxBlendShapeChannel@fbxsdk@@QEAA_NPEAVFbxShape@2@N@Z")]
        private static extern void AddTargetShapeInternal(nint pHandle, nint handle, double weight);

        internal void AddTargetShape(FbxShape lShape, double weight)
        {
            AddTargetShapeInternal(Handle, lShape.Handle, weight);
        }

        
        public FbxBlendShapeChannel(nint InHandle)
            : base(InHandle)
        {
        }
        public FbxBlendShapeChannel(FbxObject obj, string name)
            : this(CreateFromObject(obj.Handle, name))
        {
        }
    }
}
