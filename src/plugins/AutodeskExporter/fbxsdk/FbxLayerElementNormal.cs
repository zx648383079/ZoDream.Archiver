using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxLayerElementNormal : FbxLayerElement
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxLayerElementNormal@fbxsdk@@SAPEAV12@PEAVFbxLayerContainer@2@PEBD@Z")]
        private static extern nint CreateFromObject(nint pOwner, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetDirectArray@?$FbxLayerElementTemplate@VFbxVector4@fbxsdk@@@fbxsdk@@QEAAAEAV?$FbxLayerElementArrayTemplate@VFbxVector4@fbxsdk@@@2@XZ", CallingConvention = CallingConvention.ThisCall)]
        private static extern nint GetDirectArrayInternal(nint InHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetIndexArray@?$FbxLayerElementTemplate@VFbxVector4@fbxsdk@@@fbxsdk@@QEAAAEAV?$FbxLayerElementArrayTemplate@H@2@XZ", CallingConvention = CallingConvention.ThisCall)]
        private static extern nint GetIndexArrayInternal(nint InHandle);

        public FbxLayerElementNormal(nint handle)
            : base(handle)
        {
        }

        public FbxLayerElementNormal(FbxLayerContainer pOwner, string pName)
            : this(CreateFromObject(pOwner.Handle, pName))
        {
        }

        public FbxLayerElementArray<FbxVector4>? DirectArray {
            get {
                var ptr = GetDirectArrayInternal(Handle);
                return ptr == nint.Zero ? null : new FbxLayerElementArray<FbxVector4>(ptr);
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

        public void AddDirect(double x, double y, double z, double w = 0)
        {
            using var vec = new FbxVector4(x, y, z, w);
            var src = DirectArray;
            src!.Add(vec);
        }
    }

}
