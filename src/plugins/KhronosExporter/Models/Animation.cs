using System.Collections.Generic;

namespace ZoDream.KhronosExporter.Models
{
    public class Animation : LogicalChildOfRoot
    {
        /// <summary>
        /// 定义了动画的目标对象和属性。
        /// </summary>
        public IList<AnimationChannel> Channels { get; set; } = [];
        /// <summary>
        /// 定义了如何插值关键帧数据。
        /// </summary>
        public IList<AnimationSampler> Samplers { get; set; } = [];

    }
}
