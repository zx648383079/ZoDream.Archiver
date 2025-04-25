using System;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal interface IMultipartExporter : IBundleExporter, IDisposable
    {

        public bool IsEmpty { get; }

        public void Append(int entryId);

    }
}
