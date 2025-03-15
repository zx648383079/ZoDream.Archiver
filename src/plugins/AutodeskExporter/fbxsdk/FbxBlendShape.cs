using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxBlendShape : FbxDeformer
    {


        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxBlendShape@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern IntPtr CreateFromObject(IntPtr pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);
        
        [DllImport(NativeMethods.DllName, EntryPoint = "?GetBlendShapeChannelCount@FbxBlendShape@fbxsdk@@QEBAHXZ")]
        private static extern int GetBlendShapeChannelCountInternal(nint pHandle);
        
        [DllImport(NativeMethods.DllName, EntryPoint = "?AddBlendShapeChannel@FbxBlendShape@fbxsdk@@QEAA_NPEAVFbxBlendShapeChannel@2@@Z")]
        private static extern void AddBlendShapeChannelInternal(nint pHandle, nint handle);
        
        [DllImport(NativeMethods.DllName, EntryPoint = "?GetBlendShapeChannel@FbxBlendShape@fbxsdk@@QEAAPEAVFbxBlendShapeChannel@2@H@Z")]
        private static extern nint GetBlendShapeChannelInternal(nint pHandle, int channelIndex);
        
        public int BlendShapeChannelCount => GetBlendShapeChannelCountInternal(Handle);


        public FbxBlendShape(IntPtr InHandle)
            : base(InHandle)
        {
        }
        public FbxBlendShape(FbxObject obj, string name)
            : this(CreateFromObject(obj.Handle, name))
        {
        }
        internal void AddBlendShapeChannel(FbxBlendShapeChannel channel)
        {
            AddBlendShapeChannelInternal(Handle, channel.Handle);
        }


        internal FbxBlendShapeChannel GetBlendShapeChannel(int channelIndex)
        {
            var ptr = GetBlendShapeChannelInternal(Handle, channelIndex);
            return new FbxBlendShapeChannel(ptr);
        }
    }

}
