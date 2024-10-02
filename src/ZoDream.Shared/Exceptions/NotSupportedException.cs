using System;

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
