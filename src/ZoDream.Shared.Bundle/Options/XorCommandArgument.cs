using System;

namespace ZoDream.Shared.Bundle
{
    public class XorCommandArgument(byte[] keys, long maxPosition = 0) : ICommandArgument
    {
        public const string TagName = "xor";
        public byte[] Keys => keys;

        public long MaxPosition => maxPosition;

        public override string ToString()
        {
            if (maxPosition > 0)
            {
                return $"{TagName}:{Convert.ToHexString(keys)},{maxPosition}";
            }
            return $"{TagName}:{Convert.ToHexString(keys)}";
        }
    }
}
