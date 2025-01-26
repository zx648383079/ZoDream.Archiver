namespace ZoDream.SpineExporter.Models
{
    public enum MixPose
    {
        /// <summary> The timeline value is mixed with the setup pose (the current pose is not used).</summary>
        Setup,
		/// <summary> The timeline value is mixed with the current pose. The setup pose is used as the timeline value before the first key,
		/// except for timelines which perform instant transitions, such as DrawOrderTimeline or AttachmentTimeline.</summary>
		Current,
		/// <summary> The timeline value is mixed with the current pose. No change is made before the first key (the current pose is kept until the first key).</summary>
		CurrentLayered
    }
}
