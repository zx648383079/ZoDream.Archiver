using System.Collections.Generic;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleChunk: IEnumerable<string>
    {
        public string Create(string sourcePath, string outputFolder);
    }
}
