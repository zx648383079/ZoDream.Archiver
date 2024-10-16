using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZoDream.EpubExtractor
{
    public partial class EpubWriter
    {

        private static string UUID()
        {
            return Guid.NewGuid().ToString();
        }
        
        private string RenderOPF()
        {
            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>")
                .Append("<package xmlns=\"http://www.idpf.org/2007/opf\" unique-identifier=\"bookId\" version=\"2.0\">")
                .Append("<metadata xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:opf=\"http://www.idpf.org/2007/opf\">")
                .Append("<dc:identifier id=\"bookId\">urn:uuid:").Append(_uuid)
                .Append("</dc:identifier>")
                .Append("<dc:language>en</dc:language>")
                .Append("<dc:title>")
                .Append(_sectionName)
                .Append("</dc:title>")
                .Append("<meta content=\"cover-image\" name=\"cover\"/>")
                .Append("</metadata>");

            RenderManifast(sb, _fileItems, string.Empty);
            RenderSpine(sb, _chapterItems.Select(i => i.FileName));
            sb.Append("</package>");
            return sb.ToString();
        }
        private void RenderSpine(StringBuilder sb, IEnumerable<string> items)
        {
            sb.Append("<spine toc=\"ncx\">");
            foreach (var item in items)
            {
                sb.Append("<itemref idref=\"")
                    .Append(item)
                    .Append("\"/>");
            }
            sb.Append("</spine>");
        }
        private void RenderManifast(StringBuilder sb, 
            Dictionary<string, string> items, string coverImage)
        {
            sb.Append("<manifest><item href=\"toc")
                .Append(RenderSection(_sectionIndex))
                .Append(".ncx\" id=\"ncx\" media-type=\"application/x-dtbncx+xml\"/>");
            foreach (var item in items)
            {
                sb.Append("<item href=\"")
                    .Append(item.Value)
                    .Append("\" id=\"")
                    .Append(item.Key)
                    .Append("\" media-type=\"")
                    .Append(MimeMapping.MimeUtility.GetMimeMapping(item.Value))
                    .Append("\"/>");
            }
            if (string.IsNullOrWhiteSpace(coverImage))
            {
                sb.Append("<item href=\"")
                                    .Append(coverImage)
                                    .Append("\" id=\"cover-image\" media-type=\"")
                                    .Append(MimeMapping.MimeUtility.GetMimeMapping(coverImage))
                                    .Append("\"/>");
            }
            sb.Append("</manifest>");
        }
    }
}
