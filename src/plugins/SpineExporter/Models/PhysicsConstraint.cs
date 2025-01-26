using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.SpineExporter.Models
{
    public class PhysicsConstraint
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public string Bone { get; internal set; }
        public float X { get; internal set; }
        public float Y { get; internal set; }
        public float Rotate { get; internal set; }
        public float ScaleX { get; internal set; }
        public float ShearX { get; internal set; }
        public float Limit { get; internal set; }
        public float Step { get; internal set; }
        public float Inertia { get; internal set; }
        public float Strength { get; internal set; }
        public float Damping { get; internal set; }
        public float MassInverse { get; internal set; }
        public float Wind { get; internal set; }
        public float Gravity { get; internal set; }
        public float Mix { get; internal set; }
    }
}
