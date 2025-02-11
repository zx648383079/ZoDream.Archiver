namespace ZoDream.KhronosExporter.Models
{
    public class Image : LogicalChildOfRoot
    {
        public string Uri { get; set; }

        public string MimeType { get; set; }

        public int BufferView { get; set; }

    }
}
