using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.SpineExporter.Models
{
    public class IkConstraint
    {
        public string Name { get; set; }

        public int Order { get; set; }

        public string[] Bones { get; set; }
        /// <summary>
        /// Bone name
        /// </summary>
        public string Target { get; set; }

        public float Mix { get; set; }

        public int BendDirection { get; set; }
    }
}
