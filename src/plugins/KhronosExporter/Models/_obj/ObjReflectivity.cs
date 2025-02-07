using System.Numerics;

namespace ZoDream.KhronosExporter.Models
{
    internal class ObjReflectivity
    {
        public ObjFactorColor? Color { get; }

        public ObjSpectral? Spectral { get; }

        public Vector3? XYZ { get; }

        public ObjReflectivityType AmbientType { get; }

        public ObjReflectivity(ObjFactorColor color)
        {
            AmbientType = ObjReflectivityType.Color;
            Color = color;
        }

        public ObjReflectivity(ObjSpectral spectral)
        {
            AmbientType = ObjReflectivityType.Spectral;
            Spectral = spectral;
        }

        public ObjReflectivity(Vector3 xyz)
        {
            AmbientType = ObjReflectivityType.XYZ;
            XYZ = xyz;
        }
    }
}