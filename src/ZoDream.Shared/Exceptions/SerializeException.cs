using System;
using System.Runtime.CompilerServices;

namespace ZoDream.Shared
{
    public class SerializeException(string message) : Exception(message)
    {
        public static void ThrowIf(bool condition, [CallerMemberName] string message = "")
        {
            if (condition)
            {
                throw new SerializeException(message);
            }
        }

        public static void ThrowIfNot(bool condition, [CallerMemberName] string message = "")
        {
            if (!condition)
            {
                throw new SerializeException(message);
            }
        }
    }
}
