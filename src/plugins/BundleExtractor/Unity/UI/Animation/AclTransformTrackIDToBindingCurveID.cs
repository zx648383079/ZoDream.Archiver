using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class AclTransformTrackIDToBindingCurveID
    {
        public uint rotationIDToBindingCurveID;
        public uint positionIDToBindingCurveID;
        public uint scaleIDToBindingCurveID;
        public AclTransformTrackIDToBindingCurveID(IBundleBinaryReader reader)
        {
            rotationIDToBindingCurveID = reader.ReadUInt32();
            positionIDToBindingCurveID = reader.ReadUInt32();
            scaleIDToBindingCurveID = reader.ReadUInt32();
        }
    }

}
