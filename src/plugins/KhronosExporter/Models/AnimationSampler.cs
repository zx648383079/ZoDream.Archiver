namespace ZoDream.KhronosExporter.Models
{
    public class AnimationSampler : ExtraProperties
    {
        /// <summary>
        /// 引用 accessors 数组中的一个访问器，定义了关键帧的时间戳。如果为 float 则是秒，为 short/byte 则根据 min/max 归一化化处理
        /// </summary>
        public int Input {  get; set; }
        /// <summary>
        /// 引用 accessors 数组中的一个访问器，定义了关键帧的属性值。
        /// </summary>
        public int Output { get; set; }

        public AnimationInterpolationMode Interpolation { get; set; }


    }
}
