using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.KhronosExporter.Models
{
    public class ModelRoot : ExtraProperties
    {

        public string[] ExtensionsUsed { get; set; }

        public string[] ExtensionsRequired { get; set; }

        public Accessor[] Accessors {  get; set; }

        public Animation[] Animations { get; set; }

        public Asset Asset { get; set; }

        public Buffer[] Buffers { get; set; }

        public BufferView[] BufferViews {  get; set; }

        public Camera[] Cameras {  get; set; }

        public Image[] Images {  get; set; }

        public Material[] Materials {  get; set; }

        public Mesh[] Meshes {  get; set; }

        public Node[] Nodes {  get; set; }

        public TextureSampler[] Samplers {  get; set; }

        public float Scene {  get; set; }

        public Scene[] Scenes {  get; set; }

        public Skin[] Skins {  get; set; }

        public Texture[] Textures {  get; set; }
    }
}
