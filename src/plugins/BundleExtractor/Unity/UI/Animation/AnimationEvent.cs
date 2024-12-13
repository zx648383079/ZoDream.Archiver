namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class AnimationEvent
    {
        public float time;
        public string functionName;
        public string data;
        public PPtr<UIObject> objectReferenceParameter;
        public float floatParameter;
        public int intParameter;
        public int messageOptions;

        public AnimationEvent(UIReader reader)
        {
            var version = reader.Version;

            time = reader.ReadSingle();
            functionName = reader.ReadAlignedString();
            data = reader.ReadAlignedString();
            objectReferenceParameter = new PPtr<UIObject>(reader);
            floatParameter = reader.ReadSingle();
            if (version.Major >= 3) //3 and up
            {
                intParameter = reader.ReadInt32();
            }
            messageOptions = reader.ReadInt32();
        }

    }
}
