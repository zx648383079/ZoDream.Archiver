using System.Collections.Generic;

namespace ZoDream.KhronosExporter.Models
{
    /// <summary>
    /// Scene -> Nodes -> Meshes 
    /// Mesh -> Primitive -> Attributes -> Accessors -> BufferViews -> Buffers
    /// Material -> Texture -> Image
    /// </summary>
    public class ModelRoot : ExtraProperties
    {

        public Asset? Asset { get; set; }

        public int Scene { get; set; }

        public IList<Scene> Scenes { get; set; } = [];

        public IList<Node> Nodes { get; set; } = [];

        public IList<Mesh> Meshes { get; set; } = [];

        public IList<Accessor> Accessors { get; set; } = [];

        public IList<Animation>? Animations { get; set; }


        public IList<Camera>? Cameras {  get; set; }


        public IList<Material>? Materials { get; set; }



        public IList<TextureSampler>? Samplers { get; set; }

        public IList<Skin>? Skins { get; set; }

        public IList<Texture>? Textures { get; set; }


        public IList<Image>? Images { get; set; }
        public IList<BufferView> BufferViews { get; set; } = [];
        public IList<BufferSource> Buffers { get; set; } = [];



        public IList<string>? ExtensionsUsed { get; set; }

        public IList<string>? ExtensionsRequired { get; set; }
    }
}
