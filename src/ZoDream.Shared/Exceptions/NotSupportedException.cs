using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Shared
{
    /// <summary>
    /// 不支持
    /// </summary>
    /// <param name="message"></param>
    public class NotSupportedException(string message): Exception(message)
    {
    }
}
