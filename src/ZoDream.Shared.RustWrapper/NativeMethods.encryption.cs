using System.Runtime.InteropServices;

namespace ZoDream.Shared.RustWrapper
{
    internal unsafe static partial class NativeMethods
    {
        

        [DllImport(RustDllName, ExactSpelling = true)]
        public static unsafe extern EncryptorRef* find_encryptor(EncryptionID encryption);

        [DllImport(RustDllName, EntryPoint = "find_encryptor_with_key", ExactSpelling = true)]
        public static unsafe extern EncryptorRef* find_encryptor(EncryptionID encryption, byte /*const*/ * key);

        [DllImport(RustDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static unsafe extern int encrypt_encryptor(EncryptorRef* encryptor, ref InputStreamRef input, ref OutputStreamRef output, ref LoggerRef logger);

        [DllImport(RustDllName, ExactSpelling = true)]
        public static unsafe extern int decrypt_encryptor(EncryptorRef* encryptor, ref InputStreamRef input, ref OutputStreamRef output, ref LoggerRef logger);

        [DllImport(RustDllName, ExactSpelling = true)]
        public static unsafe extern void free_encryptor(EncryptorRef* encryptor);
    }
}
