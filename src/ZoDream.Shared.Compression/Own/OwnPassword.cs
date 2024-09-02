using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ZoDream.Shared.Compression.Own
{
    public class OwnPassword : IOwnKey
    {
        public OwnPassword(string password)
        {
            var buffer = Encoding.UTF8.GetBytes(password);
            _key = [..MD5.HashData(buffer), ..buffer];
        }

        private readonly byte[] _key;
        private int _position;

        public byte ReadByte()
        {
            if (_key.Length == 0)
            {
                return 0;
            }
            if (_position >= _key.Length) 
            {
                _position %= _key.Length;
            }
            return _key[_position++];
        }

        public void Seek(long len, SeekOrigin origin)
        {
            var pos = origin switch
            {
                SeekOrigin.Current => _position + len,
                SeekOrigin.End => _key.Length + len,
                _ => len
            };
            _position = (int)(pos % _key.Length);
        }

        public void Dispose()
        {
        }
    }
}
