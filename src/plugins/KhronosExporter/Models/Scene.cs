using System.Collections.Generic;

namespace ZoDream.KhronosExporter.Models
{
    public class Scene : LogicalChildOfRoot
    {
        public IList<int> Nodes { get; set; } = [];

    }
}
