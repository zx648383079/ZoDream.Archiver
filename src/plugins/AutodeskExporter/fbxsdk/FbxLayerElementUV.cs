using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxLayerElementUV : FbxLayerElement
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxLayerElementUV@fbxsdk@@SAPEAV12@PEAVFbxLayerContainer@2@PEBD@Z")]
        private static extern nint CreateFromObject(nint pOwner, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetDirectArray@?$FbxLayerElementTemplate@VFbxVector2@fbxsdk@@@fbxsdk@@QEAAAEAV?$FbxLayerElementArrayTemplate@VFbxVector2@fbxsdk@@@2@XZ", CallingConvention = CallingConvention.ThisCall)]
        private static extern nint GetDirectArrayInternal(nint InHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetIndexArray@?$FbxLayerElementTemplate@VFbxVector2@fbxsdk@@@fbxsdk@@QEAAAEAV?$FbxLayerElementArrayTemplate@H@2@XZ", CallingConvention = CallingConvention.ThisCall)]
        private static extern nint GetIndexArrayInternal(nint InHandle);

        public FbxLayerElementUV(nint handle)
            : base(handle)
        {
        }

        public FbxLayerElementUV(FbxLayerContainer pOwner, string pName)
            : this(CreateFromObject(pOwner.Handle, pName))
        {
        }

        public FbxLayerElementArray<FbxVector2>? DirectArray {
            get {
                var ptr = GetDirectArrayInternal(Handle);
                return ptr == nint.Zero ? null : new FbxLayerElementArray<FbxVector2>(ptr);
            }
        }

        public FbxLayerElementArray? IndexArray {
            get {
                var ptr = GetIndexArrayInternal(Handle);
                return ptr == nint.Zero ? null : new FbxLayerElementArray(ptr);
            }
        }

        public void AddDirect(Vector2 vec)
        {
            AddDirect(vec.X, vec.Y);
        }

        public void AddDirect(double x, double y)
        {
            using var item = new FbxVector2(x, y);
            var src = DirectArray;
            src!.Add(item);
        }
    }

}
