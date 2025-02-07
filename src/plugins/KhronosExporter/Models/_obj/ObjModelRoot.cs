using System;
using System.Collections.Generic;
using System.Numerics;

namespace ZoDream.KhronosExporter.Models
{
    internal class ObjModelRoot
    {
        public string Name { get; set; }
        public string MatFilename { get; set; }
        public List<Vector3> Vertices { get; set; } = [];
        public List<Vector3> Normals { get; set; } = [];
        public List<Vector2> Uvs { get; set; } = [];

        private Dictionary<string, ObjGeometry> _geometries = [];

        public IEnumerable<ObjGeometry> Geometries => _geometries.Values;

        public List<ObjMaterial> Materials { get; set; } = [];

        public IEnumerable<ObjGeometry> GetOrAddGeometries(params string[] names)
        {
            foreach (var name in names)
            {
                if (_geometries.TryGetValue(name, out ObjGeometry? value)) 
                {
                    yield return value;
                }
                else
                {
                    var instance = new ObjGeometry(name);
                    _geometries.Add(name, instance);
                    yield return instance;
                }
            }
        }
    }
}
