namespace ZoDream.KhronosExporter.Models
{
    public class AnimationSampler : ExtraProperties
    {
        public int Input {  get; set; }

        public AnimationInterpolationMode Interpolation { get; set; }

        public int Output { get; set; }
    }
}
