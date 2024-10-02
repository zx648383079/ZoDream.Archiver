using System.IO;

namespace ZoDream.Shared.Interfaces
{
    public interface IArchiveHeader
    {

        public void Read(Stream input);
    }
}
