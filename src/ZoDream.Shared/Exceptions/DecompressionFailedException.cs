using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Shared.Exceptions
{
    public class DecompressionFailedException(string message) : Exception(message)
    {
    }
}
