using System.Collections.Generic;
using System.Numerics;

namespace ZoDream.KhronosExporter.Models
{
    /// <summary>
    /// 光源
    /// </summary>
    public class LightsPunctual
    {
        public const string ExtensionName = "KHR_lights_punctual";

        public int? Light { get; set; }

        public IList<Light>? Lights { get; set; }
    }

    public class Light : LogicalChildOfRoot
    {

        public Vector3? Color { get; set; }

        public float Intensity { get; set; } = 1;

        public LightType Type { get; set; } = LightType.Directional;

        public float? Range { get; set; }

        public LightSpot? Spot { get; set; }
    }

    public enum LightType
    {
        Directional,
        Point,
        Spot
    }

    public class LightSpot
    {
        public float InnerConeAngle { get; set; }

        public float? OuterConeAngle { get; set; }
    }
}
