using System.IO;

namespace ZoDream.Shared.Bundle
{
    public interface ILanguageScheme<T>
    {
        public void Create(Stream stream, T data);
        public T? Open(Stream stream, string filePath, string fileName);
    }
}
