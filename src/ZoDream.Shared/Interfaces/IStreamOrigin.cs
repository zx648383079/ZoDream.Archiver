using System.IO;

namespace ZoDream.Shared.Interfaces
{
    /// <summary>
    /// 用来判断是否是一个封装流，能获取上一级的源流
    /// </summary>
    public interface IStreamOrigin
    {

        public Stream BaseStream { get; }
    }
}
