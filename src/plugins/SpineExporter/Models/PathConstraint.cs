namespace ZoDream.SpineExporter.Models
{
    public class PathConstraint
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public string[] Bones { get; set; }
        public string Target { get; set; }
        public PositionMode PositionMode { get; set; }
        public SpacingMode SpacingMode { get; set; }
        public RotateMode RotateMode { get; set; }
        public float OffsetRotation { get; set; }
        public float Position { get; set; }
        public float Spacing { get; set; }
        public float RotateMix { get; set; }
        public float TranslateXMix { get; set; }
        public float TranslateYMix { get; set; }
    }
}
