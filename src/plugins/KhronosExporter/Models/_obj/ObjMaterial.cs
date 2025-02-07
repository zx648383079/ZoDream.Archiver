namespace ZoDream.KhronosExporter.Models
{
    internal class ObjMaterial
    {
        /// <summary>
        ///  matname
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Ka: Ambient Color
        /// </summary>
        public ObjReflectivity Ambient { get; set; } = new(new ObjFactorColor());
        /// <summary>
        /// Kd: Diffuse Color
        /// </summary>
        public ObjReflectivity Diffuse { get; set; } = new(new ObjFactorColor(0.5f));
        /// <summary>
        /// map_Kd: Diffuse texture file path
        /// </summary>
        public string DiffuseTextureFile { get; set; }
        /// <summary>
        /// map_Ka: Ambient texture file path
        /// </summary>
        public string AmbientTextureFile { get; set; }
        /// <summary>
        /// norm: Normal texture file path
        /// </summary>
        public string NormalTextureFile { get; set; }
        /// <summary>
        /// Ks: specular reflectivity of the current material
        /// </summary>
        public ObjReflectivity Specular { get; set; } = new ObjReflectivity(new ObjFactorColor());
        /// <summary>
        /// Tf: transmission filter: Any light passing through the object 
        /// is filtered by the transmission filter
        /// </summary>
        public ObjReflectivity Filter { get; set; }
        /// <summary>
        /// Ke: emissive color
        /// </summary>
        public ObjReflectivity Emissive { get; set; }
        /// <summary>
        /// illum: illum_# 0 ~ 10
        /// </summary>
        public int? Illumination { get; set; }
        /// <summary>
        /// d: the dissolve for the current material.
        /// </summary>
        public ObjDissolve Dissolve { get; set; }
        /// <summary>
        /// Tr: Transparency
        /// </summary>
        public ObjTransparency Transparency { get; set; }
        /// <summary>
        /// Ns: specularShininess 0 ~ 1000
        /// </summary>
        public double SpecularExponent { get; set; }
        /// <summary>
        /// sharpness value 0 ~ 1000, The default is 60
        /// </summary>
        public int? Sharpness { get; set; }
        /// <summary>
        /// 0.001 ~ 10
        /// </summary>
        public double? OpticalDensity { get; set; }

        public double GetAlpha()
        {
            if (Dissolve != null)
            {
                return Dissolve.Factor;
            }
            if (Transparency != null)
            {
                return (1.0 - Transparency.Factor);
            }
            return 1.0;
        }
    }
}