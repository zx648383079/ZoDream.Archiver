using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxNodeAttribute : FbxObject
    {
        public enum EType
        {
            eUnknown,
            eNull,
            eMarker,
            eSkeleton,
            eMesh,
            eNurbs,
            ePatch,
            eCamera,
            eCameraStereo,
            eCameraSwitcher,
            eLight,
            eOpticalReference,
            eOpticalMarker,
            eNurbsCurve,
            eTrimNurbsSurface,
            eBoundary,
            eNurbsSurface,
            eShape,
            eLODGroup,
            eSubDiv,
            eCachedEffect,
            eLine
        };

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetNodeCount@FbxNodeAttribute@fbxsdk@@QEBAHXZ")]
        private static extern int GetNodeCountInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetNode@FbxNodeAttribute@fbxsdk@@QEBAPEAVFbxNode@2@H@Z")]
        private static extern IntPtr GetNodeInternal(IntPtr handle, int pIndex);

        private delegate EType GetAttributeTypeDelegate(IntPtr handle);
        private GetAttributeTypeDelegate? GetAttributeTypeInternal;


        private readonly nint _color;

        public EType AttributeType => GetAttributeTypeInternal is not null ?
            GetAttributeTypeInternal(pHandle) : EType.eUnknown;
        public int NodeCount => GetNodeCountInternal(pHandle);

        public Vector4 Color {
            get {
                Vector3 v = FbxProperty.GetDouble3(_color);
                return new Vector4(v.X, v.Y, v.Z, 1.0f);
            }
        }

        public FbxNodeAttribute() { }
        public FbxNodeAttribute(IntPtr InHandle)
            : base(InHandle)
        {
            GetAttributeTypeInternal = Marshal.GetDelegateForFunctionPointer<GetAttributeTypeDelegate>(Marshal.ReadIntPtr(vTable + 0xB8));
            _color = pHandle + 0x78;
        }

        public FbxNode? GetNode(int index)
        {
            if (index >= NodeCount)
                return null;

            IntPtr ptr = GetNodeInternal(pHandle, index);
            return ptr == IntPtr.Zero ? null : new FbxNode(ptr);
        }
    }

}
