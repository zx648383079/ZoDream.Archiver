using System.IO;

namespace ZoDream.Shared.Language
{
    public interface ILanguageScheme<T> 
        where T : IBytecode
    {
        public void Create(Stream stream, T data);
        public T? Open(Stream stream);
    }
}
