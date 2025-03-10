namespace ZoDream.KhronosExporter.Models
{
    public class Image : LogicalChildOfRoot
    {
        /// <summary>
        /// data:image/png;base64,
        /// </summary>
        public string Uri { get; set; }

        public string MimeType { get; set; }

        public int? BufferView { get; set; }

    }
}
