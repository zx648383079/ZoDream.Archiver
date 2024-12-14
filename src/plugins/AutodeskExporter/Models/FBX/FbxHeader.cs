using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.AutodeskExporter.Models
{
    internal class FbxHeader
    {

        internal const string Signature = "Kaydara FBX Binary  \x00\x1A\x00";

        public uint Version { get; set; }
    }
}
