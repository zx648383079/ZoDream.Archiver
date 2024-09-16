using System;

namespace ZoDream.Shared
{
    /// <summary>
    /// 解压密码错误
    /// </summary>
    /// <param name="message"></param>
    public class CryptographicException(string message) : Exception(message)
    {
    }
}
