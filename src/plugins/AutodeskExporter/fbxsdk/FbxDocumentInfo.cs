using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxDocumentInfo : FbxObject
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxDocumentInfo@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern nint CreateFromManager(nint pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        private readonly nint _mOriginal_ApplicationVendor; // FbxPropertyT<FbxString>
        private readonly nint _mOriginal_ApplicationName; // FbxPropertyT<FbxString>
        private readonly nint _mOriginal_ApplicationVersion; // FbxPropertyT<FbxString>

        private readonly nint _mLastSaved_ApplicationVendor; // FbxPropertyT<FbxString>
        private readonly nint _mLastSaved_ApplicationName; // FbxPropertyT<FbxString>
        private readonly nint _mLastSaved_ApplicationVersion; // FbxPropertyT<FbxString>

        private readonly nint _mTitle; // FbxString
        private readonly nint _mSubject; // FbxString

        public FbxDocumentInfo(nint handle)
            : base(handle)
        {
            _mOriginal_ApplicationVendor = GetPropertyPtr(0xA8);
            _mOriginal_ApplicationName = GetPropertyPtr(0xB8);
            _mOriginal_ApplicationVersion = GetPropertyPtr(0xC8);

            _mLastSaved_ApplicationVendor = GetPropertyPtr(0x108);
            _mLastSaved_ApplicationName = GetPropertyPtr(0x118);
            _mLastSaved_ApplicationVersion = GetPropertyPtr(0x128);

            _mTitle = GetPropertyPtr(0x158);
            _mSubject = GetPropertyPtr(0x160);
        }

        public FbxDocumentInfo(FbxManager Manager, string Name)
            : this(CreateFromManager(Manager.Handle, Name))
        {
        }

        public string OriginalApplicationVendor { get => FbxProperty.GetString(_mOriginal_ApplicationVendor); set => FbxProperty.Set(_mOriginal_ApplicationVendor, value); }
        public string OriginalApplicationName { get => FbxProperty.GetString(_mOriginal_ApplicationName); set => FbxProperty.Set(_mOriginal_ApplicationName, value); }
        public string OriginalApplicationVersion { get => FbxProperty.GetString(_mOriginal_ApplicationVersion); set => FbxProperty.Set(_mOriginal_ApplicationVersion, value); }

        public string LastSavedApplicationVendor { get => FbxProperty.GetString(_mLastSaved_ApplicationVendor); set => FbxProperty.Set(_mLastSaved_ApplicationVendor, value); }
        public string LastSavedApplicationName { get => FbxProperty.GetString(_mLastSaved_ApplicationName); set => FbxProperty.Set(_mLastSaved_ApplicationName, value); }
        public string LastSavedApplicationVersion { get => FbxProperty.GetString(_mLastSaved_ApplicationVersion); set => FbxProperty.Set(_mLastSaved_ApplicationVersion, value); }

        public string Title { get => FbxString.Get(_mTitle); set => FbxString.Assign(_mTitle, value); }
        public string Subject { get => FbxString.Get(_mSubject); set => FbxString.Assign(_mSubject, value); }
    }

}
