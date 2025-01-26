using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.SpineExporter.Models;

namespace ZoDream.SpineExporter
{
    public partial class AtlasReader
    {
        public IEnumerable<AtlasPage>? Deserialize(string content, string fileName)
        {
            using var reader = new StringReader(content);
            foreach (var item in Deserialize(reader))
            {
                yield return item;
            }
        }


        

        public string Serialize(IEnumerable<AtlasPage> data, string fileName)
        {
            foreach (var res in data)
            {
                var sb = new StringBuilder();
                sb.AppendLine(res.Name)
                    .AppendLine($"size: {res.Width},{res.Height}")
                    .AppendLine("format: RGBA8888")
                    .AppendLine("filter: Linear,Linear")
                    .AppendLine("repeat: none");
                foreach (var item in res.Items)
                {
                    sb.AppendLine(item.Name)
                        .AppendLine("  rotate: " + (item.Rotate == 90 ? "true" : "false"))
                        .AppendLine($"  xy: {item.X}, {item.Y}")
                        .AppendLine($"  size: {item.Width}, {item.Height}")
                        .AppendLine($"  orig: {item.Width}, {item.Height}")
                        .AppendLine("  offset: 0, 0")
                        .AppendLine("  index: -1")
                        ;
                }
                return sb.ToString();
            }
            return string.Empty;
        }

    }
}
