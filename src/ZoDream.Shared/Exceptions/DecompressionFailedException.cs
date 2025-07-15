using System;

namespace ZoDream.Shared.Exceptions
{
    public class DecompressionFailedException(string message) : Exception(message)
    {
    }
}
