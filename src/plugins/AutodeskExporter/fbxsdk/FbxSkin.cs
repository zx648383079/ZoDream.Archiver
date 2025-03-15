using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxSkin : FbxDeformer
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxSkin@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern IntPtr CreateFromManager(IntPtr pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxSkin@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern IntPtr CreateFromObject(IntPtr pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?AddCluster@FbxSkin@fbxsdk@@QEAA_NPEAVFbxCluster@2@@Z")]
        private static extern bool AddClusterInternal(IntPtr pHandle, IntPtr pCluster);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetClusterCount@FbxSkin@fbxsdk@@QEBAHXZ")]
        private static extern int GetClusterCountInternal(IntPtr pHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetCluster@FbxSkin@fbxsdk@@QEAAPEAVFbxCluster@2@H@Z")]
        private static extern IntPtr GetClusterInternal(IntPtr pHandle, int pIndex);

        public int ClusterCount => GetClusterCountInternal(Handle);

        public IEnumerable<FbxCluster> Clusters {
            get {
                for (int i = 0; i < ClusterCount; i++)
                {
                    var item = GetCluster(i);
                    if (item is not null)
                    {
                        yield return item;
                    }
                }
            }
        }

        public FbxSkin(IntPtr InHandle)
            : base(InHandle)
        {
        }

        public FbxSkin(FbxDeformer deformer)
            : this(deformer.Handle)
        {
        }

        public FbxSkin(FbxManager manager, string name)
            : this(CreateFromManager(manager.Handle, name))
        {
        }

        public FbxSkin(FbxObject obj, string name)
            : this(CreateFromObject(obj.Handle, name))
        {
        }

        public bool AddCluster(FbxCluster cluster)
        {
            return AddClusterInternal(Handle, cluster.Handle);
        }

        public FbxCluster? GetCluster(int index)
        {
            IntPtr ptr = GetClusterInternal(Handle, index);
            return ptr == IntPtr.Zero ? null : new FbxCluster(ptr);
        }
    }

}
