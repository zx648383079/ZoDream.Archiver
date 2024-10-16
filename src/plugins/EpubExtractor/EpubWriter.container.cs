using System.Text;

namespace ZoDream.EpubExtractor
{
    public partial class EpubWriter
    {
        
        private string RenderContainer()
        {
            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>")
                .Append("<container xmlns=\"urn:oasis:names:tc:opendocument:xmlns:container\" version=\"1.0\">")
                .Append("<rootfiles>");
            for (var i = 0; i <= _sectionIndex; i++)
            {
                sb.Append("<rootfile full-path=\"OEBPS/content")
                    .Append(RenderSection(i)).Append(".opf\" media-type=\"application/oebps-package+xml\"/>");
            }
            sb.Append("</rootfiles></container>");
            return sb.ToString();
        }

    }
}
