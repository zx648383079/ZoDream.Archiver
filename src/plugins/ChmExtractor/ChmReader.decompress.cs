using System;
using System.Buffers;
using System.IO;
using ZoDream.ChmExtractor.Models;
using ZoDream.Shared.IO;

namespace ZoDream.ChmExtractor
{
    public partial class ChmReader
    {
        private void DecompressRegion(ChmUnitInfo info, Stream output)
        {
            if (info.Space == 0)
            {
                new PartialStream(_reader.BaseStream, _header.DataOffset + info.Start, info.Length).CopyTo(output);
                return;
            }
            var remaining = (ulong)info.Length;
            var offset = (ulong)info.Start;
            do
            {
                var len = DecompressRegion(offset, remaining, output);
                offset += len;
                remaining -= len;
            }
            while (remaining > 0);
        }

        private ulong DecompressRegion(ulong start, ulong length, Stream output)
        {
            var nBlock = start / _header.ResetTable.BlockLen;
            var nOffset = start % _header.ResetTable.BlockLen;
            var nLen = Math.Min(length, _header.ResetTable.BlockLen - nOffset);

            var tempBlock = (int)nBlock % _header.CacheNumBlocks;
            if (_header.CacheBlockIndices[tempBlock] == nBlock
                && _header.CacheBlocks[tempBlock] is not null) 
            {
                output.Write(_header.CacheBlocks[tempBlock], (int)nOffset, (int)nLen);
                return nLen;
            }
            if (_header.LzxDecoder is null)
            {
                var windowSize = CountZeroBits(_header.WindowSize) - 1;
                _header.LzxLastBlock = -1;
                _header.LzxDecoder = new(windowSize);
            }
            var gotLen = DecompressBlock(nBlock);
            if (gotLen < nLen)
            {
                nLen = gotLen;
            }
            // TODO
            output.Write(_header.CacheBlocks[tempBlock], (int)nOffset, (int)nLen);
            return nLen;
        }
        private ulong DecompressBlock(ulong block)
        {
            var buffer = ArrayPool<byte>.Shared.Rent((int)_header.ResetTable.BlockLen + 6144);
            var blockAlign = (uint)(block % _header.ResetBlkCount);
            var indexSlot = 0;
            var blockPosition = 0L;
            var blockLength = 0;
            if ((uint)block - blockAlign <= _header.LzxLastBlock && block >= (ulong)_header.LzxLastBlock)
            {
                blockAlign = (uint)(block - (ulong)_header.LzxLastBlock);
            }
            if (blockAlign != 0)
            {
                for (var i = blockAlign; i > 0; i--)
                {
                    var curBlockIndex = block - i;
                    if ((ulong)_header.LzxLastBlock != curBlockIndex)
                    {
                        if (curBlockIndex % _header.ResetBlkCount == 0)
                        {
                            _header.LzxDecoder!.Reset();
                        }
                        indexSlot = (int)(curBlockIndex % (ulong)_header.CacheNumBlocks);
                        if (_header.CacheBlocks[indexSlot] is null)
                        {
                            _header.CacheBlocks[indexSlot] = new byte[_header.ResetTable.BlockLen];
                        }
                        _header.CacheBlockIndices[indexSlot] = curBlockIndex;

                        (blockPosition, blockLength) = GetBlockRange((long)curBlockIndex);
                        _reader.BaseStream.Position = blockPosition;
                        _reader.BaseStream.ReadExactly(buffer, 0, blockLength);
                        _header.LzxDecoder.Decompress(
                            buffer, 
                            blockLength,
                            _header.CacheBlocks[indexSlot], 
                            (int)_header.ResetTable.BlockLen);

                        _header.LzxLastBlock = (int)curBlockIndex;
                    }
                }
            } else if (block % _header.ResetBlkCount == 0)
            {
                _header.LzxDecoder!.Reset();
            }
            indexSlot = (int)(block % (ulong)_header.CacheNumBlocks);
            if (_header.CacheBlocks[indexSlot] is null)
            {
                _header.CacheBlocks[indexSlot] = new byte[_header.ResetTable.BlockLen];
            }
            _header.CacheBlockIndices[indexSlot] = block;
            (blockPosition, blockLength) = GetBlockRange((long)block);
            _reader.BaseStream.Position = blockPosition;
            _reader.BaseStream.ReadExactly(buffer, 0, blockLength);
            _header.LzxDecoder.Decompress(
                buffer,
                blockLength,
                _header.CacheBlocks[indexSlot],
                (int)_header.ResetTable.BlockLen);

            _header.LzxLastBlock = (int)block;
            ArrayPool<byte>.Shared.Return(buffer);
            return _header.ResetTable.BlockLen;
        }

        private (long, int) GetBlockRange(long block)
        {
            var start = 0L;
            var length = 0L;
            _reader.BaseStream.Position = _header.DataOffset + _header.RtUnit.Start + _header.ResetTable.TableOffset + block * 8;
            start = (long)_reader.ReadUInt64();
            if (block < _header.ResetTable.BlockCount - 1)
            {
                length = _reader.ReadInt64();
            } else
            {
                length = (long)_header.ResetTable.CompressedLen;
            }
            length -= start;
            start += _header.DataOffset + _header.CnUnit.Start;
            return (start, (int)length);
        }

        private static int CountZeroBits(uint val)
        {
            var bit = 1;
            var idx = 1;
            while (bit != 0 && (val & bit) == 0)
            {
                bit <<= 1;
                ++idx;
            }
            return bit == 0 ? 0 : idx;
        }
    }
}
