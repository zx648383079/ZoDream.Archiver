using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.Compression.Zip;
using ZoDream.Shared.Interfaces;

namespace ZoDream.EpubExtractor
{
    public partial class EpubWriter
    {
        
        private string RenderNCX()
        {
            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>")
                .Append("<ncx xmlns=\"http://www.daisy.org/z3986/2005/ncx/\">")
                .Append("<head>")
                .Append("<meta content=\"urn:uuid:")
                .Append(_uuid)
                .Append("\" name=\"dtb:uid\"/>")
                .Append("<meta content=\"1\" name=\"dtb:depth\"/>")
                .Append("<meta content=\"0\" name=\"dtb:totalPageCount\"/>")
                .Append("<meta content=\"0\" name=\"dtb:maxPageNumber\"/>")
                .Append("</head>");
            RenderSection(sb, _sectionName, _chapterItems);
            return sb.Append("</ncx>").ToString();
        }


        private void RenderSection(StringBuilder sb, string sectionName, 
            IEnumerable<ChapterItem> items)
        {
            sb.Append("<docTitle><text>")
                .Append(sectionName)
                .Append("</text></docTitle><navMap>");
            var i = 0;
            foreach (var item in items)
            {
                sb.Append("<navPoint id=\"navPoint-14\" playOrder=\"")
                    .Append(++i)
                    .Append("\"><navLabel><text>")
                    .Append(item.Title)
                    .Append("</text></navLabel><content src=\"")
                    .Append(item.FileName)
                    .Append("\"/></navPoint>");
            }
            sb.Append("</navMap>");
        }
    }
}
