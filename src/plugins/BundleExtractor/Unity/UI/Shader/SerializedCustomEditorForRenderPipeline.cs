using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SerializedCustomEditorForRenderPipeline
    {
        public string customEditorName;
        public string renderPipelineType;

        public SerializedCustomEditorForRenderPipeline(UIReader reader)
        {
            customEditorName = reader.ReadAlignedString();
            renderPipelineType = reader.ReadAlignedString();
        }
    }

}
