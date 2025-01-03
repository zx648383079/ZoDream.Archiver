using ZoDream.Shared.Interfaces;

namespace ZoDream.Archiver.ViewModels
{
    public interface IEntryConfiguration
    {
        public void Load(IEntryService service, object options);

        public void Unload(IEntryService service, object options);
    }
}
