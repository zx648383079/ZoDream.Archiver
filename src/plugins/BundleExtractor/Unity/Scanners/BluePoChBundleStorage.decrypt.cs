using System.IO;
using System.Text;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class BluePoChElementScanner
    {
        private Stream DecryptReverse1999(Stream input, string fullPath)
        {
            var buffer = input.ReadBytes(7);
            input.Position = 0;
            if (Encoding.ASCII.GetString(buffer) == "UnityFS")
            {
                return input;
            }
            var key = GetAbEncryptKey(Path.GetFileNameWithoutExtension(fullPath));
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] ^= key;
            }
            if (Encoding.ASCII.GetString(buffer) != "UnityFS")
            {
                return input;
            }
            return new Reverse1999Stream(input, key);
        }

        private byte GetAbEncryptKey(string md5Name)
        {
            byte key = 0;
            foreach (var c in md5Name)
            {
                key += (byte)c;
            }
            return (byte)(key + (byte)(2 * ((key & 1) + 1)));
        }
    }
}
