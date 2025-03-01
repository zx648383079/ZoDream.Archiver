namespace ZoDream.KhronosExporter.Models
{
    public class Animation : LogicalChildOfRoot
    {
        public AnimationChannel[] Channels {  get; set; }

        public AnimationSampler[] Samplers { get; set; }

    }
}
