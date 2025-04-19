using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public class AnimationClipBindingConstant
    {
        public GenericBinding[] GenericBindings { get; set; }
        public PPtr<Object>[] CurveMapping { get; set; }

    }
}
