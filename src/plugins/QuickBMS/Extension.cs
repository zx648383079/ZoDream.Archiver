using ZoDream.Shared.Bundle;

namespace ZoDream.QuickBMS
{
    public static class Extension
    {
        extension(IBundleBinaryReader reader)
        {
            public uint ReadLongLong()
            {
                var res = reader.ReadUInt32();
                reader.Position += 4;
                return res;
            }
        }
    }
}
