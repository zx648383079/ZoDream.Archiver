using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxGeometry : FbxGeometryBase
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?AddDeformer@FbxGeometry@fbxsdk@@QEAAHPEAVFbxDeformer@2@@Z")]
        private static extern int AddDeformerInternal(nint pHandle, nint pDeformer);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetDeformerCount@FbxGeometry@fbxsdk@@QEBAHW4EDeformerType@FbxDeformer@2@@Z")]
        private static extern int GetDeformerCountInternal(nint pHandle, FbxDeformer.EDeformerType pType);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetDeformer@FbxGeometry@fbxsdk@@QEBAPEAVFbxDeformer@2@HW4EDeformerType@32@PEAVFbxStatus@2@@Z")]
        private static extern nint GetDeformerInternal(nint pHandle, int pIndex, FbxDeformer.EDeformerType pType, nint pStatus);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetShapeChannel@FbxGeometry@fbxsdk@@QEAAPEAVFbxAnimCurve@2@HHPEAVFbxAnimLayer@2@_NPEAVFbxStatus@2@@Z")]
        private static extern nint GetShapeChannelInternal(nint pHandle, int pBlendShapeIndex, int pBlendShapeChannelIndex, nint handle, bool pCreateAsNeeded);

        public FbxGeometry(nint InHandle)
            : base(InHandle)
        {
        }

        internal FbxAnimCurve GetShapeChannel(int pBlendShapeIndex, int pBlendShapeChannelIndex, FbxAnimLayer pLayer, bool pCreateAsNeeded = false)
        {
            var ptr = GetShapeChannelInternal(Handle, pBlendShapeIndex, pBlendShapeChannelIndex, pLayer.Handle, pCreateAsNeeded);
            return new FbxAnimCurve(ptr);
        }


        public int AddDeformer(FbxDeformer deformer)
        {
            return AddDeformerInternal(Handle, deformer.Handle);
        }

        public int GetDeformerCount(FbxDeformer.EDeformerType type)
        {
            return GetDeformerCountInternal(Handle, type);
        }

        public T? GetDeformer<T>(int index, FbxDeformer.EDeformerType type)
            where T : FbxDeformer
        {
            nint ptr = GetDeformerInternal(Handle, index, type, nint.Zero);
            return ptr == nint.Zero ? null : (T)Activator.CreateInstance(typeof(T), ptr);
        }
    }

}
