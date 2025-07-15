using System;
using System.Runtime.InteropServices;
using ZoDream.Shared;

namespace ZoDream.AutodeskExporter
{
    internal class FbxNative : IDisposable
    {
        private nint _handle = nint.Zero;
        /// <summary>
        /// 是否需要自动执行销毁
        /// </summary>
        protected bool _leaveFree;
        private bool _disposedValue;

        public nint Handle { get => _handle; protected set => _handle = value; }

        public FbxNative()
        {
            
        }

        public FbxNative(nint InHandle)
        {
            _handle = InHandle;
            Expectation.ThrowIfNot(Handle != nint.Zero);
        }


        /// <summary>
        /// 根据偏移获取有效的指针地址
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        protected nint GetPropertyPtr(int offset)
        {
            return Marshal.ReadIntPtr(_handle, offset);
        }



        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    FbxUtils.FbxFree(_handle);
                }
                _disposedValue = true;
            }
        }


        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        ~FbxNative()
        {
            if (_leaveFree)
            {
                Dispose(disposing: false);
            }
        }
    }
}
