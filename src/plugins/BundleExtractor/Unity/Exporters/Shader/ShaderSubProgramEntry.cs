using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class ShaderSubProgramEntry
    {
        public int Offset;
        public int Length;
        public int Segment;

        public ShaderSubProgramEntry(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();
            Offset = reader.ReadInt32();
            Length = reader.ReadInt32();
            if (version.GreaterThanOrEquals(2019, 3)) //2019.3 and up
            {
                Segment = reader.ReadInt32();
            }
        }
    }
}
