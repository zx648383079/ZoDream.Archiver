using System.Threading;
using System.Threading.Tasks;

namespace ZoDream.Shared.Interfaces
{
    public interface IBundleReader
    {

        public void ExtractTo(string folder, CancellationToken token = default);
    }
}
