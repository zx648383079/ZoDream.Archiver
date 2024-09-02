using System.IO;

namespace ZoDream.Shared.Compression.Own
{
    public class OwnMultipleKey(params IOwnKey[] items) : IOwnKey
    {
        

        public byte ReadByte()
        {
            var total = 0;
            foreach (var item in items) 
            {
                total += item.ReadByte();
            }
            return (byte)(total % 256);
        }

        public void Seek(long len, SeekOrigin origin)
        {
            foreach (var item in items)
            {
                item.Seek(len, origin);
            }
        }

        public void Dispose()
        {
            foreach (var item in items)
            {
                item.Dispose();
            }
        }
    }
}
