﻿using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxNull : FbxNodeAttribute
    {
        internal enum ELook
        {
            eNone,
            eCross,
        };

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxNull@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern nint CreateFromManager(nint pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxNull@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern nint CreateFromObject(nint pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

        private readonly nint _size;
        private readonly nint _look;
        public double Size { get => FbxProperty.GetDouble(_size); set => FbxProperty.Set(_size, value); }
       
        public ELook Look { get => FbxProperty.GetEnum<ELook>(_look); set => FbxProperty.SetEnum(_look, (int)value); }

        public FbxNull(FbxManager Manager, string pName)
            : this(CreateFromManager(Manager.Handle, pName))
        {
        }

        public FbxNull(nint InHandle)
            : base(InHandle)
        {
            _size = GetPropertyPtr(0x88);
            _look = GetPropertyPtr(0x98);
        }

        public FbxNull(FbxObject Object, string pName)
            : this (CreateFromObject(Object.Handle, pName))
        {
        }
    }

}
