using ZoDream.Shared.Bundle;

namespace UnityEngine
{
    public class MotionNeighborList
    {
        public uint[] NeighborArray;

        public MotionNeighborList(IBundleBinaryReader reader)
        {
            NeighborArray = reader.ReadArray(r => r.ReadUInt32());
        }
    }
}
