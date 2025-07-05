using System.IO;
using System.Net;
using System.Threading.Tasks;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Net
{
    public class NetExecutor : IBundleExecutor
    {
        const long SplitMinSize = 1024 * 1024;
        private readonly INetService _service = new NetService(); 

        public virtual bool CanExecute(IBundleRequest request)
        {
            return request is INetRequest;
        }

        public async Task ExecuteAsync(IBundleRequest request, IBundleContext context)
        {
            if (request is INetRequest n)
            {
                await ExecuteAsync(n, context);
            }
        }

        public async Task ExecuteAsync(INetRequest request, IBundleContext context)
        {
            request.Token.Emit(new BundleChangedEventArgs()
            {
                Status = BundleStatus.Sending
            });
            var data = request is IRequestContext r ? r : new RequestContext()
            {
                RequestId = request.RequestId,
                Source = request.Source,
                Output = request.Output,
                Token = request.Token,
            };
            var res = await _service.SendAsync(data);
            if (res.StatusCode != HttpStatusCode.OK)
            {
                request.Token.Emit(new BundleChangedEventArgs()
                {
                    Status = BundleStatus.Failed
                });
                return;
            }
            var fileName = _service.GetFileName(res);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = request.SuggestedName ?? NetService.GetFileName(request.Source);
            }
            var length = _service.GetContentLength(res);
            var output = Path.Combine(request.Output, fileName);
            request.Token.Emit(new BundleChangedEventArgs()
            {
                Length = length,
                Name = fileName,
                OutputPath = output,
                Status = BundleStatus.Receiving
            });
            using var fs = new ConcurrentStream(File.Create(output));
            if (length > 0)
            {
                fs.SetLength(length);
            }
            using INetReceiver receiver = length > SplitMinSize && _service.GetAcceptRange(res) ?
            new ChunkReceiver(context.Service.Get<ITemporaryStorage>(), 
            _service, res, data, fs) 
            : new NetReceiver(_service, res, fs);
            await receiver.StartAsync(request.Token);
            request.Token.Emit(new BundleChangedEventArgs()
            {
                Status = BundleStatus.Completed,
            });
        }
    }
}
