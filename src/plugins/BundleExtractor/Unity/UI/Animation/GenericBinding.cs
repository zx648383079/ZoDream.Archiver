using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class GenericBinding
    {
        public UnityVersion version;
        public uint path;
        public uint attribute;
        public PPtr<UIObject> script;
        public ElementIDType typeID;
        public byte customType;
        public byte isPPtrCurve;
        public byte isIntCurve;

        public GenericBinding() { }

        public GenericBinding(IBundleBinaryReader reader)
        {
            version = reader.Get<UnityVersion>();
            path = reader.ReadUInt32();
            attribute = reader.ReadUInt32();
            script = new PPtr<UIObject>(reader);
            if (version.GreaterThanOrEquals(5, 6)) //5.6 and up
            {
                typeID = (ElementIDType)reader.ReadInt32();
            }
            else
            {
                typeID = (ElementIDType)reader.ReadUInt16();
            }
            customType = reader.ReadByte();
            isPPtrCurve = reader.ReadByte();
            if (version.GreaterThanOrEquals(2022, 1)) //2022.1 and up
            {
                isIntCurve = reader.ReadByte();
            }
            reader.AlignStream();
        }

    }
}
