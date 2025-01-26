using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.SpineExporter.Models
{
    public class PathConstraintSpacingTimeline: PathConstraintPositionTimeline
    {
        public PathConstraintSpacingTimeline(int frameCount): base(frameCount)
        {
            
        }
        public override int PropertyId => ((int)TimelineType.PathConstraintSpacing << 24) + PathConstraintIndex;
    }
}
