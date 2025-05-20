using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ZoDream.Shared.Net
{
    /// <summary>
    /// Represents a thread-safe, ordered collection of objects.
    /// With thread-safe multi-thread adding and single-thread consuming methodology (N producers - 1 consumer)
    /// </summary>
    /// <remarks>
    /// <typeparam name="T">Specifies the type of elements in the ConcurrentDictionary.</typeparam>
    /// </remarks>
    [DebuggerTypeProxy(typeof(IReadOnlyCollection<>))]
    [DebuggerDisplay("Count = {Count}")]
    internal class ConcurrentPacketBuffer : IReadOnlyCollection<Packet>, IDisposable
    {
        private volatile bool _disposed;
        private long _bufferSize = long.MaxValue;
        private readonly SemaphoreSlim _consumeLocker = new(0);
        private readonly PauseTokenSource _addingBlocker = new();
        private readonly PauseTokenSource _flushBlocker = new();
        private readonly ConcurrentQueue<Packet> _items = new();

        public long BufferSize {
            get => _bufferSize;
            set => _bufferSize = (value <= 0) ? long.MaxValue : value;
        }
        public ConcurrentPacketBuffer()
        {
            
        }

        public ConcurrentPacketBuffer(long size)
        {
            BufferSize = size;
        }

        public IEnumerator<Packet> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _items.Count;
        public bool IsAddingCompleted => _addingBlocker.IsPaused;
        public bool IsEmpty => _items.IsEmpty;

        public Packet[] ToArray()
        {
            return [.. _items];
        }

        public async Task<bool> TryAddAsync(Packet item)
        {
            try
            {
                await _addingBlocker.WaitWhilePausedAsync().ConfigureAwait(false);
                _flushBlocker.Pause();
                _items.Enqueue(item);
                _consumeLocker.Release();
                StopAddingIfLimitationExceeded(item.Length);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task WaitTryTakeAsync(Func<Packet, Task> callbackTask, CancellationToken token)
        {
            try
            {
                await _consumeLocker.WaitAsync(token).ConfigureAwait(false);
                if (_items.TryDequeue(out var item) && item != null)
                {
                    await callbackTask(item).ConfigureAwait(false);
                }
            }
            finally
            {
                ResumeAddingIfEmpty();
            }
        }

        private void StopAddingIfLimitationExceeded(long packetSize)
        {
            if (BufferSize < packetSize * Count)
            {
                StopAdding();
            }
        }

        private void ResumeAddingIfEmpty()
        {
            if (IsEmpty)
            {
                _flushBlocker.Resume();
                ResumeAdding();
            }
        }

        public async Task WaitToComplete()
        {
            await _flushBlocker.WaitWhilePausedAsync().ConfigureAwait(false);
        }

        public void StopAdding()
        {
            _addingBlocker.Pause();
        }

        public void ResumeAdding()
        {
            _addingBlocker.Resume();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                StopAdding();
                _consumeLocker.Dispose();
                _addingBlocker.Resume();
            }
        }
    }
}
