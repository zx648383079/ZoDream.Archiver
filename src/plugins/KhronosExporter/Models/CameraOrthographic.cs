namespace ZoDream.KhronosExporter.Models
{
    public class CameraOrthographic : ExtraProperties
    {
        public float Xmag { get; set; }

        public float Ymag { get; set; }
        public float Zfar { get; set; }

        public float Znear {  get; set; }
    }
}
