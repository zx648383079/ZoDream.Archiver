using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal abstract class ACLClip
    {
        public virtual bool IsSet => false;
        public virtual uint CurveCount => 0;
        public abstract void Read(IBundleBinaryReader reader);
    }
}
