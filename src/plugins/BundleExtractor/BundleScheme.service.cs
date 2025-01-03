using System;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme : IDisposable
    {
        public BundleScheme(IEntryService service)
        {
            Service = service;
            Initialize();
        }

        public IEntryService Service {  get; private set; }

        public void Dispose()
        {
        }
    }
}
