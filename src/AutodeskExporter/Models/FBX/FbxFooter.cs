using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.AutodeskExporter.Models
{
    internal class FbxFooter
    {
        internal const string IdSignature = "\xFA\xBC\xAB\x09\xD0\xC8\xD4\x66\xB1\x76\xFB\x83\x1C\xF7\x26\x7E";
        internal const string Signature = "\xF8\x5A\x8C\x6A\xDE\xF5\xD9\x7E\xEC\xE9\x0C\xE3\x75\x8F\x29\x0B";

        public uint Version { get; internal set; }
    }
}
