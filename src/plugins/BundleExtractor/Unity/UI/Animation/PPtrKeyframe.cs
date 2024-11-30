namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class PPtrKeyframe : IYamlWriter
    {
        public float time;
        public PPtr<UIObject> value;

        public PPtrKeyframe(float time, PPtr<UIObject> value)
        {
            this.time = time;
            this.value = value;
        }

        public PPtrKeyframe(UIReader reader)
        {
            time = reader.ReadSingle();
            value = new PPtr<UIObject>(reader);
        }
        //public YAMLNode ExportYAML(int[] version)
        //{
        //    var node = new YAMLMappingNode();
        //    node.Add(nameof(time), time);
        //    node.Add(nameof(value), value.ExportYAML(version));
        //    return node;
        //}
    }

}
