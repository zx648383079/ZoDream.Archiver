using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.Bundle;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocDrawOrderGroupOffsetPtr
    {
        public uint ObjectSourcesBeginIndices {  get; private set; }
        public uint ObjectSourcesCounts {  get; private set; }
        public uint ObjectSourcesTotalCounts {  get; private set; }
        public uint MaximumDrawOrders {  get; private set; }
        public uint MinimumDrawOrders {  get; private set; }

        public void Read(IBundleBinaryReader reader)
        {
            ObjectSourcesBeginIndices = reader.ReadUInt32();
            ObjectSourcesCounts = reader.ReadUInt32();
            ObjectSourcesTotalCounts = reader.ReadUInt32();
            MaximumDrawOrders = reader.ReadUInt32();
            MinimumDrawOrders = reader.ReadUInt32();
        }
    }
    internal class MocDrawOrderGroupOffset
    {
        public int[] ObjectSourcesBeginIndices { get; private set; }
        public int[] ObjectSourcesCounts { get; private set; }
        public int[] ObjectSourcesTotalCounts { get; private set; }
        public uint[] MaximumDrawOrders { get; private set; }
        public uint[] MinimumDrawOrders { get; private set; }

        public void Read(IBundleBinaryReader reader, int count)
        {
            var ptr = new MocDrawOrderGroupOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            ObjectSourcesBeginIndices = reader.ReadArray(ptr.ObjectSourcesBeginIndices, count, () => reader.ReadInt32());
            ObjectSourcesCounts = reader.ReadArray(ptr.ObjectSourcesCounts, count, () => reader.ReadInt32());
            ObjectSourcesTotalCounts = reader.ReadArray(ptr.ObjectSourcesTotalCounts, count, () => reader.ReadInt32());
            MaximumDrawOrders = reader.ReadArray(ptr.MaximumDrawOrders, count, () => reader.ReadUInt32());
            MinimumDrawOrders = reader.ReadArray(ptr.MinimumDrawOrders, count, () => reader.ReadUInt32());
            

            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
