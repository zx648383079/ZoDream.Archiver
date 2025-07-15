using System.Runtime.CompilerServices;

namespace ZoDream.Shared
{
    public class VersionException(string message) : NotSupportedException(message)
    {
        public static void ThrowIf(bool condition, [CallerMemberName] string message = "")
        {
            if (condition)
            {
                throw new VersionException(message);
            }
        }

        public static void ThrowIfNot(bool condition, [CallerMemberName] string message = "")
        {
            if (!condition)
            {
                throw new VersionException(message);
            }
        }
    }
}
