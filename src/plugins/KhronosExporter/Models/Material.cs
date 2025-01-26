using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.KhronosExporter.Models
{
    public class Material : LogicalChildOfRoot
    {


        public MaterialPbrMetallicRoughness PbrMetallicRoughness { get; set; }

        public MaterialNormalTextureInfo NormalTexture {  get; set; }

        public MaterialOcclusionTextureInfo OcclusionTexture {  get; set; }

        public TextureInfo EmissiveTexture {  get; set; }

        public float[] EmissiveFactor {  get; set; }

        public AlphaMode AlphaMode {  get; set; }

        public float AlphaCutoff {  get; set; }

        public bool DoubleSided {  get; set; }
    }
}
