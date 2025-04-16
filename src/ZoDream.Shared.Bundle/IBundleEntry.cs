using System.Collections.Generic;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleEntry : IReadOnlyEntry
    {
        public long Id { get; }

        public int Type { get; }

        public long Offset { get; }
    }

    public interface IBundleEntrySource : IList<IBundleEntry>
    {
        public string Name { get; }

        public string FullPath { get; }
    }
}
