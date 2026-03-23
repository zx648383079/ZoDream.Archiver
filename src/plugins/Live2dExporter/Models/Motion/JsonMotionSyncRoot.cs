namespace ZoDream.Live2dExporter.Models
{
    internal class JsonMotionSyncRoot
    {
        public int Version { get; set; }

        public JsonMotionSyncMeta Meta { get; set; }

        public JsonMotionSyncSetting[] Settings { get; set; }
    }
}
