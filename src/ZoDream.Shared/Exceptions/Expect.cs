using System;
using System.Runtime.CompilerServices;

namespace ZoDream.Shared
{
    /// <summary>
    /// 预期不符处理
    /// </summary>
    public static class Expectation
    {
        public static void ThrowIf(bool condition, [CallerMemberName] string message = "")
        {
            if (condition)
            {
                throw new Exception(message);
            }
        }

        public static void ThrowIfNot(bool condition, [CallerMemberName] string message = "")
        {
            if (!condition)
            {
                throw new Exception(message);
            }
        }


        public static void ThrowIfNotSignature(bool condition, [CallerMemberName] string message = "")
        {
            if (!condition)
            {
                throw new SignatureException(message);
            }
        }

        public static void ThrowIfNotSignature(string expected, string actual, [CallerMemberName] string message = "")
        {
            if (expected != actual)
            {
                throw new SignatureException($"{message} '{actual}' is not '{expected}'");
            }
        }

        public static void ThrowIfNotVersion(bool condition, [CallerMemberName] string message = "")
        {
            if (!condition)
            {
                throw new VersionException(message);
            }
        }
    }
}
