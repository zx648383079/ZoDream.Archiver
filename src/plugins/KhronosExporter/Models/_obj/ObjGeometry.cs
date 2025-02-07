using System;
using System.Collections.Generic;

namespace ZoDream.KhronosExporter.Models
{
    internal class ObjGeometry
    {
        public string Id { get; }

        public List<ObjFace> Faces { get; } = [];

        public ObjGeometry(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }
            Id = id;
        }
    }
}
