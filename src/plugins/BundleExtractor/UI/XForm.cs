using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.UI
{
    public record XForm<K>(K T, Vector4 Q, K S)
    {
    }
}
