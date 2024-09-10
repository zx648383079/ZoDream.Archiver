using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Shared.RustWrapper
{
    internal unsafe static partial class NativeMethods
    {
        const string __DllName = "libzodream";

        [DllImport(__DllName, ExactSpelling = true)]
        public static unsafe extern void with_concat(
        byte /*const*/ * fst,
        byte /*const*/ * snd,
        FunCallRef cb);

        [DllImport(__DllName, ExactSpelling = true)]
        public static unsafe extern byte* concat(
        byte /*const*/ * fst,
        byte /*const*/ * snd);

        [DllImport(__DllName, ExactSpelling = true)]
        public static unsafe extern void free_char_p(byte* _string);

        [DllImport(__DllName, ExactSpelling = true)]
        public static unsafe extern Int32 /*const*/ * max(IntArrayRef xs);

        [return: MarshalAs(UnmanagedType.FunctionPtr)]
        [DllImport(__DllName, ExactSpelling = true)]
        public static unsafe extern RustCallFn returns_a_fn_ptr();


    }
}
