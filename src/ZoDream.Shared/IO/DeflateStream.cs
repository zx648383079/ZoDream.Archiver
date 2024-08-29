using System.IO;

namespace ZoDream.Shared.IO
{
    public abstract class DeflateStream(Stream stream): ReadOnlyStream(stream)
    {
    }
}
