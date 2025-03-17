using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using ZoDream.ChmExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.ChmExtractor
{
    public partial class ChmReader
    {
        private void Initialize()
        {
            _reader.BaseStream.Seek(_basePosition, SeekOrigin.Begin);
            ReadHeader();
            ReadItspHeader();
            var uiLzxc = new ChmUnitInfo();
            if (!ReadUnitInfo(_header.RtUnit, ChmUnitInfo.CHMU_RESET_TABLE)
                || _header.RtUnit.Space == 1
                || !ReadUnitInfo(_header.CnUnit, ChmUnitInfo.CHMU_CONTENT)
                || _header.CnUnit.Space == 1
                || !ReadUnitInfo(uiLzxc, ChmUnitInfo.CHMU_LZXC_CONTROLDATA)
                || uiLzxc.Space == 1)
            {
                _header.CompressionEnabled = false;
            }
            if (_header.CompressionEnabled)
            {
                _reader.BaseStream.Position = _header.DataOffset + _header.RtUnit.Start;
                if (!ReadResetTable())
                {
                    _header.CompressionEnabled = false;
                }
            }
            if (_header.CompressionEnabled)
            {
                _reader.BaseStream.Position = _header.DataOffset + uiLzxc.Start;
                if (!ReadControlData(uiLzxc.Length))
                {
                    _header.CompressionEnabled = false;
                }
            }
            InitializeCache(0, 5);
        }

        private void InitializeCache(int paramType, int paramVal)
        {
            if (paramType != 0)
            {
                return;
            }
            if (paramVal == _header.CacheNumBlocks)
            {
                return;
            }
            var blockItems = new byte[paramVal][];
            var indicesItems = new ulong[paramVal];
            if (_header.CacheBlocks is not null)
            {
                for (var i = 0; i < _header.CacheNumBlocks; i++)
                {
                    var slot = (int)(_header.CacheBlockIndices[i] % (ulong)paramVal);
                    if (_header.CacheBlocks[i] is not null)
                    {
                        if (blockItems[slot] is not null)
                        {
                            // _header.CacheBlocks[i] = null;
                        } else
                        {
                            blockItems[slot] = _header.CacheBlocks[i];
                            indicesItems[slot] = _header.CacheBlockIndices[i];
                        }
                    }
                }
            }
            _header.CacheBlocks = blockItems;
            _header.CacheBlockIndices = indicesItems;
            _header.CacheNumBlocks = paramVal;
        }

        private bool ReadControlData(long length)
        {
            var header = _header.ControlData;
            header.Size = _reader.ReadUInt32();
            Debug.Assert(_reader.ReadBytes(4).Equal(ChmLzxcControlData.Signature));
            header.Version = _reader.ReadUInt32();
            header.ResetInterval = _reader.ReadUInt32();
            header.WindowSize = _reader.ReadUInt32();
            header.WindowsPerReset = _reader.ReadUInt32();
            if (length >= 0x1c)
            {
                header.Unknown_18 = _reader.ReadUInt32();
            }
            if (header.Version == 2)
            {
                header.ResetInterval *= 0x8000;
                header.WindowSize *= 0x8000;
            }
            return header.ResetInterval != 0 && header.WindowSize != 0 && header.WindowSize != 1 && header.ResetInterval % (header.WindowSize / 2) == 0;
        }

        private bool ReadUnitInfo(ChmUnitInfo data, string fileName)
        {
            var current = _header.IndexRoot;
            while (current != -1)
            {
                _reader.BaseStream.Position = _header.DirOffset + current * _header.BlockLen;
                if (_reader.BaseStream.Length - _reader.BaseStream.Position < _header.BlockLen)
                {
                    return false;
                }
                var signature = _reader.ReadBytes(4);
                if (signature.Equal(ChmPmglHeader.Signature))
                {
                    var header = ReadPmglHeader(false);
                    var end = _reader.BaseStream.Position + _header.BlockLen - header.FreeSpaceLength;
                    while (_reader.BaseStream.Position < end)
                    {
                        var pos = _reader.BaseStream.Position;
                        var len = _reader.Read7BitEncodedInt64();
                        if (len > 512)
                        {
                            break;
                        }
                        if (len == fileName.Length &&
                            Encoding.ASCII.GetString(_reader.ReadBytes((int)len)).Equals(fileName, StringComparison.OrdinalIgnoreCase))
                        {
                            data.FileName = fileName;
                            data.Space = (int)_reader.Read7BitEncodedInt64();
                            data.Start = _reader.Read7BitEncodedInt64();
                            data.Length = _reader.Read7BitEncodedInt64();
                            break;
                        }
                        _reader.Read7BitEncodedInt64();
                        _reader.Read7BitEncodedInt64();
                        _reader.Read7BitEncodedInt64();
                    }
                    return !string.IsNullOrEmpty(data.FileName);
                } else if (signature.Equal(ChmPmgiHeader.Signature))
                {
                    var header = ReadPmgiHeader(false);
                    current = -1;
                    var end = _reader.BaseStream.Position + _header.BlockLen - header.FreeSpaceLength;
                    while (_reader.BaseStream.Position < end)
                    {
                        var len = _reader.Read7BitEncodedInt64();
                        if (len > 512)
                        {
                            break;
                        }
                        if (len == fileName.Length &&
                            Encoding.ASCII.GetString(_reader.ReadBytes((int)len)).Equals(fileName, StringComparison.OrdinalIgnoreCase))
                        {
                            current = (int)_reader.Read7BitEncodedInt64();
                            continue;
                        }
                        break;
                    }
                } else
                {
                    return false;
                }
            }
            return false;
        }

        private void ReadItspHeader(bool hasSignature = true)
        {
            _reader.BaseStream.Seek((long)_header.Header.DirOffset, SeekOrigin.Begin);
            var header = _header.ItspHeader;
            if (hasSignature)
            {
                Debug.Assert(_reader.ReadBytes(4).Equal(ChmItspHeader.Signature));
            }
            header.Version = _reader.ReadInt32();
            header.DirectoryHeaderLength1 = _reader.ReadInt32();
            header.Unknown_000c = _reader.ReadInt32();
            header.DirectoryChunkSize = _reader.ReadUInt32();
            header.QuickRefSectionDensity = _reader.ReadInt32();
            header.IndexTreeDepth = _reader.ReadInt32();
            header.RootIndexChunkNumber = _reader.ReadInt32();
            header.FirstPMGLChunkNumber = _reader.ReadInt32();
            header.LastPMGLChunkNumber = _reader.ReadInt32();
            header.NumBlocks = _reader.ReadUInt32();
            header.DirectoryChunkCount = _reader.ReadUInt32();
            header.LangId = (WindowsLanguageId)_reader.ReadUInt32();
            _reader.BaseStream.ReadExactly(header.SystemUuid);
            header.DirectoryHeaderLength2 = _reader.ReadInt32();
            header.Unknown_0048 = _reader.ReadUInt32();
            header.Unknown_004C = _reader.ReadUInt32();
            header.Unknown_0050 = _reader.ReadUInt32();
            if (header.Version != 1 || header.DirectoryHeaderLength1 != 0x54)
            {
                throw new Exception("data is error");
            }
        }

        private void ReadHeader(bool hasSignature = true)
        {
            var header = _header.Header;
            if (hasSignature)
            {
                Debug.Assert(_reader.ReadBytes(4).Equal(ChmFileHeader.Signature));
            }
            header.Version = _reader.ReadInt32();
            header.HeaderLen = _reader.ReadInt32();
            header.Unknown_000c = _reader.ReadInt32();
            header.LastModified = BinaryPrimitives.ReadUInt32BigEndian(_reader.ReadBytes(4));
            header.LangId = (WindowsLanguageId)_reader.ReadUInt32();
            _reader.BaseStream.ReadExactly(header.DirUuid);
            _reader.BaseStream.ReadExactly(header.StreamUuid);
            header.UnknownOffset = _reader.ReadUInt64();
            header.UnknownLen = _reader.ReadUInt64();
            header.DirOffset = _reader.ReadUInt64();
            header.DirLen = _reader.ReadUInt64();

            if ((header.Version == 2 && header.HeaderLen < 0x58) || 
                (header.Version == 3 && header.HeaderLen < 0x60))
            {
                throw new Exception("data is error");
            }
            if (header.Version == 3)
            {
                header.DataOffset = _reader.ReadUInt64();
            } else
            {
                header.DataOffset = header.DirOffset + header.DirLen;
            }
        }
        private ChmPmgiHeader ReadPmgiHeader(bool hasSignature = true)
        {
            var header = new ChmPmgiHeader();
            if (hasSignature)
            {
                Debug.Assert(_reader.ReadBytes(4).Equal(ChmPmgiHeader.Signature));
            }
            header.FreeSpaceLength = _reader.ReadUInt32();
            return header;
        }
        private ChmPmglHeader ReadPmglHeader(bool hasSignature = true)
        {
            var header = new ChmPmglHeader();
            if (hasSignature)
            {
                Debug.Assert(_reader.ReadBytes(4).Equal(ChmPmglHeader.Signature));
            }
            header.FreeSpaceLength = _reader.ReadUInt32();
            header.Unknown_0008 = _reader.ReadUInt32();
            header.PrevChunkNumber = _reader.ReadInt32();
            header.NextChunkNumber = _reader.ReadInt32();
            return header;
        }

        private bool ReadResetTable()
        {
            var header = _header.ResetTable;
            header.Version = _reader.ReadUInt32();
            header.BlockCount = _reader.ReadUInt32();
            header.Unknown = _reader.ReadUInt32();
            header.TableOffset = _reader.ReadUInt32();
            header.UncompressedLen = _reader.ReadUInt64();
            header.CompressedLen = _reader.ReadUInt64();
            header.BlockLen = _reader.ReadUInt64();
            return header.Version == 2;
        }

        private IEnumerable<FileArchiveEntry> ReadHeaderSectionTableEntry()
        {
            var pos = _reader.BaseStream.Position;
            _reader.BaseStream.Seek(_header.DirOffset, SeekOrigin.Begin);
            for (var i = 0; i < _header.ItspHeader.DirectoryChunkCount; i++)
            {
                foreach (var item in ReadListingChunk(_header.ItspHeader.DirectoryChunkSize))
                {
                    yield return item;
                }
            }
            //var res = ReadHeaderSection();
            ////_reader.BaseStream.Seek(pos, SeekOrigin.Begin);
            //return res;
        }

        private FileArchiveEntry[] ReadHeaderSection()
        {
            var magic = _reader.ReadBytes(4);
            if (magic.Equal([0xFE, 0x01, 0x00, 0x00]))
            {
                _reader.ReadUInt32();
                // 总文件尺寸
                var fileSize = _reader.ReadUInt32();
                _reader.ReadUInt32();
                _reader.ReadUInt32();
                return [];
            } 
            else if (magic.Equal(ChmItspHeader.Signature))
            {
                ReadItspHeader(false);
                var res = new List<FileArchiveEntry>();
                for (var i = 0; i < _header.ItspHeader.DirectoryChunkCount; i++)
                {
                    // Debug.WriteLine($"{i}: 0x{reader.BaseStream.Position:X}");
                    res.AddRange(ReadListingChunk(_header.ItspHeader.DirectoryChunkSize));
                }
                return [.. res];
            } else
            {
                throw new Exception("error");
            }
        }

        private FileArchiveEntry ReadDirectoryListingEntry()
        {
            var nameLength = _reader.Read7BitEncodedInt();
            var data = new ChmUnitInfo
            {
                FileName = Encoding.ASCII.GetString(_reader.ReadBytes(nameLength)),
                Space = (int)_reader.Read7BitEncodedInt64(),
                Start = _reader.Read7BitEncodedInt64(),
                Length = _reader.Read7BitEncodedInt64()
            };
            return new FileArchiveEntry(data.FileName, data.Start, data.Length, data.Space != 0);
        }


        private FileArchiveEntry[] ReadListingChunk(uint directoryChunkSize)
        {
            var entryPos = _reader.BaseStream.Position;
            var magic = _reader.ReadBytes(4);
            if (magic.Equal(ChmPmglHeader.Signature))
            {
                var header = ReadPmglHeader(false);
                var pos = _reader.BaseStream.Position;
                entryPos += directoryChunkSize - 2;
                _reader.BaseStream.Seek(entryPos, SeekOrigin.Begin);
                var directoryListingEntryCount = _reader.ReadUInt16();
                _reader.BaseStream.Seek(entryPos - (header.FreeSpaceLength - 2), SeekOrigin.Begin);
                for (var i = 0; i < (header.FreeSpaceLength - 2) / 2; i++)
                {
                    var offsets = _reader.ReadUInt16();
                }
                _reader.BaseStream.Seek(pos, SeekOrigin.Begin);
                var res = new FileArchiveEntry[directoryListingEntryCount];
                for (var i = 0; i < directoryListingEntryCount; i++)
                {
                    res[i] = ReadDirectoryListingEntry();
                }
                _reader.BaseStream.Seek(entryPos + 2, SeekOrigin.Begin);
                return res;
            } else if (magic.Equal(ChmPmgiHeader.Signature))
            {
                var header = ReadPmgiHeader(false);
                var pos = _reader.BaseStream.Position;

                entryPos += directoryChunkSize - 2;
                _reader.BaseStream.Seek(entryPos, SeekOrigin.Begin);
                var directoryIndexEntryCount = _reader.ReadUInt16();
                _reader.BaseStream.Seek(entryPos - (header.FreeSpaceLength - 2), SeekOrigin.Begin);
                for (var i = 0; i < (header.FreeSpaceLength - 2) / 2; i++)
                {
                    var offsets = _reader.ReadUInt16();
                }

                _reader.BaseStream.Seek(pos, SeekOrigin.Begin);
                var res = new FileArchiveEntry[directoryIndexEntryCount];
                for (var i = 0; i < directoryIndexEntryCount; i++)
                {
                    res[i] = ReadDirectoryListingEntry();
                }
                _reader.BaseStream.Seek(entryPos + 2, SeekOrigin.Begin);
                return res;
            } else
            {
                throw new Exception("error");
            }
        }

        private void ReadNameListEntry()
        {
            var nameLength = _reader.ReadUInt16();
            var name = Encoding.Unicode.GetString(_reader.ReadBytes(nameLength * 2));

            _reader.BaseStream.Seek(2, SeekOrigin.Current);
        }

        private void ReadNameListFile()
        {
            var fileLengthWords = _reader.ReadUInt16();
            var entriesInFile = _reader.ReadUInt16();
            for (var i = 0; i < entriesInFile; i++)
            {
                ReadNameListEntry();
            }
            _reader.BaseStream.Seek(0x2E, SeekOrigin.Current);
        }

    }
}
