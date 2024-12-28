
using ZoDream.Shared.Interfaces;

namespace ZoDream.Archiver.ViewModels
{
    public class UnknownEntryStream: IEntryStream
    {
        public static readonly UnknownEntryStream Instance = new();
    }
}
