using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxLayerElementVertexColor : FbxLayerElement
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxLayerElementVertexColor@fbxsdk@@SAPEAV12@PEAVFbxLayerContainer@2@PEBD@Z")]
        private static extern nint CreateFromObject(nint pOwner, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetDirectArray@?$FbxLayerElementTemplate@VFbxColor@fbxsdk@@@fbxsdk@@QEAAAEAV?$FbxLayerElementArrayTemplate@VFbxColor@fbxsdk@@@2@XZ", CallingConvention = CallingConvention.ThisCall)]
        private static extern nint GetDirectArrayInternal(nint InHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetIndexArray@?$FbxLayerElementTemplate@VFbxColor@fbxsdk@@@fbxsdk@@QEAAAEAV?$FbxLayerElementArrayTemplate@H@2@XZ", CallingConvention = CallingConvention.ThisCall)]
        private static extern nint GetIndexArrayInternal(nint InHandle);

        public FbxLayerElementVertexColor(nint handle)
            : base(handle)
        {
        }

        public FbxLayerElementVertexColor(FbxLayerContainer pOwner, string pName)
            : this(CreateFromObject(pOwner.Handle, pName))
        {
        }

        public FbxLayerElementArray<FbxColor>? DirectArray {
            get {
                var ptr = GetDirectArrayInternal(Handle);
                return ptr == nint.Zero ? null : new FbxLayerElementArray<FbxColor>(ptr);
            }
        }

        public FbxLayerElementArray? IndexArray {
            get {
                var ptr = GetIndexArrayInternal(Handle);
                return ptr == nint.Zero ? null : new FbxLayerElementArray(ptr);
            }
        }

        public void AddDirect(Vector4 vec)
        {
            AddDirect(vec.X, vec.Y, vec.Z, vec.W);
        }

        public void AddDirect(double x, double y, double z, double w = 1)
        {
            using var color = new FbxColor(x, y, z, w);
            var src = DirectArray;
            src!.Add(color);
        }
    }

}
