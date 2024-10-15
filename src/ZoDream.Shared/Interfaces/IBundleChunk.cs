using System.Collections.Generic;

namespace ZoDream.Shared.Interfaces
{
    public interface IBundleChunk: IEnumerable<string>
    {
        public string Create(string sourcePath, string outputFolder);
    }
}
