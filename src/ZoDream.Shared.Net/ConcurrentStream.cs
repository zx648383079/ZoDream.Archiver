using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ZoDream.Shared.Net
{
    /// <summary>
    /// 并发写入流
    /// </summary>
    public class ConcurrentStream : Stream, IDisposable, IAsyncDisposable
    {
        public ConcurrentStream(Stream output)
        {
            BaseStream = output;
            var task = Task.Factory.StartNew(
                function: Watcher,
                cancellationToken: _watcherCancelSource.Token,
                creationOptions: TaskCreationOptions.LongRunning,
                scheduler: TaskScheduler.Default);
            task.Unwrap();
        }
        public ConcurrentStream(Stream output, long maxMemoryBufferBytes)
            : this(output)
        {
            _cache.BufferSize = maxMemoryBufferBytes;
        }

        public ConcurrentStream(Stream output, long length, long maxMemoryBufferBytes)
            : this(output, maxMemoryBufferBytes)
        {
            SetLength(length);
        }

        private readonly Stream BaseStream;
        private readonly ConcurrentPacketBuffer _cache = new();
        private volatile bool _disposed;
        private CancellationTokenSource _watcherCancelSource = new();

        public override bool CanRead => false;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => BaseStream.Length;

        public override long Position {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        public long MaxMemoryBufferBytes {
            get => _cache.BufferSize;
            set => _cache.BufferSize = value;
        }

        #region 任务控制
        public Exception LastException { get; private set; }
        public TaskStatus Status { get; private set; } = TaskStatus.Created;
        
        public bool IsCompleted => Status == TaskStatus.RanToCompletion || Status == TaskStatus.Faulted || Status == TaskStatus.Canceled;

        public bool IsCanceled => Status == TaskStatus.Canceled;

        public bool IsCompletedSuccessfully => Status == TaskStatus.RanToCompletion;

        public bool IsFaulted => Status == TaskStatus.Faulted;

        internal void StartState() => Status = TaskStatus.Running;

        internal void CompleteState() => Status = TaskStatus.RanToCompletion;

        internal void CancelState() => Status = TaskStatus.Canceled;
        #endregion

        public override int Read(byte[] buffer, int offset, int count)
        {
            return BaseStream.Read(buffer, offset, count);
        }

        public async Task WriteAsync(long position, byte[] bytes, int length, bool fireAndForget = true)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(length, bytes.Length);
            if (IsFaulted && LastException is not null)
            {
                throw LastException;
            }
            await _cache.TryAddAsync(new Packet(position, bytes, length)).ConfigureAwait(false);

            if (fireAndForget == false)
            {
                await FlushAsync().ConfigureAwait(false);
            }
        }

        private async Task Watcher()
        {
            try
            {
                StartState();
                while (!_watcherCancelSource.IsCancellationRequested)
                {
                    await _cache.WaitTryTakeAsync(WritePacketOnFile, _watcherCancelSource.Token)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                CancelState();
            }
            catch (Exception ex)
            {
                LastException = ex;
                _watcherCancelSource.Cancel(false);
            }
            finally
            {
                await Task.Yield();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin); ;
        }

        public override void SetLength(long value)
        {
            BaseStream.SetLength(value);
        }

        private async Task WritePacketOnFile(Packet packet)
        {
            Seek(packet.Position, SeekOrigin.Begin);
            await BaseStream.WriteAsync(packet.Data).ConfigureAwait(false);
            packet.Dispose();
        }

        public override void Flush()
        {
            _ = FlushAsync();
        }

        /// <summary>
        /// 请不要直接使用方法
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            BaseStream.Write(buffer, offset, count);
        }

        public new async Task FlushAsync()
        {
            await _cache.WaitToComplete().ConfigureAwait(false);
            if (CanRead)
            {
                await BaseStream.FlushAsync().ConfigureAwait(false);
            }

            GC.Collect();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _watcherCancelSource.Cancel(); // request the cancellation
                BaseStream?.Dispose();
                _cache.Dispose();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                _disposed = true;
                await _watcherCancelSource.CancelAsync().ConfigureAwait(false);
                await BaseStream.DisposeAsync().ConfigureAwait(false);
                _cache.Dispose();
            }
        }

      
    }
}
