using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class PPtrKeyframe
    {
        public float time;
        public PPtr<UIObject> value;

        public PPtrKeyframe(float time, PPtr<UIObject> value)
        {
            this.time = time;
            this.value = value;
        }

        public PPtrKeyframe(IBundleBinaryReader reader)
        {
            time = reader.ReadSingle();
            value = new PPtr<UIObject>(reader);
        }

    }

}
