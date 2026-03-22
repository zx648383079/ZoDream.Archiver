using System.IO;

namespace ZoDream.Shared.Interfaces
{
    public interface IManifestReader<T>
    {
        public T? ReadFrom(Stream input);
        public T? ReadFrom(string fileName);
    }

    public interface IManifestWriter<T>
    {
        public void WriteTo(Stream output, T data);
        public void WriteTo(string outputPath, T data);
    }
}
