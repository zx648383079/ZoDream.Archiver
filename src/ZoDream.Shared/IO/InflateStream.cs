using System.IO;

namespace ZoDream.Shared.IO
{
    public abstract class InflateStream(Stream stream) : ReadOnlyStream(stream)
    {
    }
}
