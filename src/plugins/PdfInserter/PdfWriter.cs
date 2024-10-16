using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZoDream.Shared.Drawing;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.PdfInserter
{
    public class PdfWriter : IArchiveWriter
    {
        public PdfWriter(Stream stream, IArchiveOptions options)
        {
            _stream = stream;
            _options = options;
            WriteHeader();
        }

        private readonly Stream _stream;
        private readonly IArchiveOptions _options;
        private readonly List<long> _blockEntryItems = [];
        private readonly List<int> _pageItems = [];

        public IReadOnlyEntry AddEntry(string name, string fullPath)
        {
            using var fs = File.OpenRead(fullPath);
            return AddEntry(name, fs);
        }

        public IReadOnlyEntry AddEntry(string name, Stream input)
        {
            using var image = SKImage.FromEncodedData(input);
            using var output = new MemoryStream();
            image.Encode(output, SKEncodedImageFormat.Jpeg, 100);
            WriteImage(image.Width, image.Height, output);
            return new ReadOnlyEntry(name, output.Length);
        }

        private void WriteText(string text)
        {
            WriteString(
                "4 0 obj",
                "<< /Type /Page",
                "/Parent 3 0 R",
                "/MediaBox [0 0 612 792]",
                "/Contents 5 0 R",
                "/Resources << /ProcSet 6 0 R",
                "/Font << /F1 7 0 R >>",
                ">>",
                ">>",
                "endobj",

                "5 0 obj",
                "<< /Length 73 >>",
                "stream",
                "BT",
                "/F1 24 Tf",
                "100 100 Td",
                "(Hello World) Tj",
                "ET",
                "endstream",
                "endobj",

                "6 0 obj",
                "[/PDF /Text]",
                "endobj",

                "7 0 obj",
                "<< /Type /Font",
                "/Subtype /Type1",
                "/Name /F1",
                "/BaseFont /Helvetica",
                "/Encoding /MacRomanEncoding",
                ">>",
                "endobj"
            );

        }

        private void WriteImage(int width, int height, Stream image)
        {
            var index = _blockEntryItems.Count + 1;
            WriteBlock(
                "<< /Type /Page",
                "/Parent 3 0 R",
                $"/Resources {index + 1} 0 R",
                $"/MediaBox [0 0 {width} {height}]",
                $"/Contents {index + 3} 0 R",
                ">>"
            );
            WriteBlock(
                "<< /ProcSet [/PDF /ImageB]",
                $"/XObject << /Im1 {index + 2} 0 R >>",
                ">>"
            );
            index = TryAddBlock();
            WriteString(
                index + " 0 obj",
                "<< /Type /XObject",
                "/Subtype /Image",
                "/Width " + width,
                "/Height " + height,
                "/ColorSpace /DeviceRGB",
                "/BitsPerComponent 8",
                "/Length " + image.Length,
                "/Filter /DCTDecode",
                ">>",
                "stream",
                string.Empty
            );
            image.CopyTo(_stream);
            WriteString(
                "endstream",
                "endobj"
            );
            string[] items = [
                "q", // Save graphics state
                "1 0 0 1 0 0 cm", // 132 0 0 132 45 140 cm: Translate to (45,140) and scale by 132
                "/Im1 Do", // Paint image
                "Q", // Restore graphics state
            ];
            var data = string.Join('\n', items);
            WriteBlock(
                $"<< /Length {data.Length} >>",
                "stream",
                data,
                "endstream"
            );
        }
        /// <summary>
        /// 文档信息字典
        /// </summary>
        private void WriteInformation()
        {
            WriteBlock(
                "<<",
                "/Title (PDF Explained Example)",
                "/Author (John Whitington)",
                "/Producer (Manually Created)",
                "/ModDate (D:20110313002346Z)",
                "/CreationDate (D:2011)",
                ">>"
            );
        }

        private void WriteHeader()
        {
            WriteString("%PDF−1.5");
            WriteBlock(
                "<< /Type /Catalog",
                "/Outlines 2 0 R",
                "/Pages 3 0 R",
                ">>"
            );
            WriteBlock(
                "<< /Type /Outlines",
                "/Count 0",
                ">>"
            );

            // 第三部分为占位，要等到最后再写入
            _blockEntryItems.Add(0);
        }

        private void WriteFooter()
        {
            // 写入第三部分
            _blockEntryItems[2] = WriteBlock(3, 
                "<< /Type /Pages",
                "/Kids ["+ string.Join(' ', _pageItems.Select(i => $"{i} 0 R")) +"]",
                "/Count " + _pageItems.Count,
                ">>"
            );

            var pos = _stream.Position + 1;
            WriteString("xref",
                "0 " + _blockEntryItems.Count,
                "0000000000 65535 f"
            );
            WriteString(
                _blockEntryItems.Select(i => $"{i:d10} 00000 n")
                .ToArray()
            );
            WriteString(
                "trailer",
                "<< /Size " + _blockEntryItems.Count,
                "/Root 1 0 R",
                ">>",
                "startxref",
                pos.ToString(),
                "%%EOF"
            );
        }

        /// <summary>
        /// 准备插入下一个部分
        /// </summary>
        /// <returns>下一部分的序号</returns>
        private int TryAddBlock()
        {
            var index = _blockEntryItems.Count + 1;
            _blockEntryItems.Add(_stream.Position + 1);
            return index;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lines"></param>
        /// <returns>编号</returns>
        private int WriteBlock(params string[] lines)
        {
            var index = _blockEntryItems.Count + 1;
            var pos = WriteBlock(index, lines);
            _blockEntryItems.Add(pos);
            return index;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="lines"></param>
        /// <returns>位置</returns>
        private long WriteBlock(int index, params string[] lines)
        {
            var pos = _stream.Position + 1;
            WriteString([
                index +" 0 obj",
                ..lines,
                "endobj"
            ]);
            if (lines[0].EndsWith("/Page"))
            {
                _pageItems.Add(index);
            }
            return pos;
        }

        private void WriteString(params string[] lines)
        {
            if (lines.Length == 0)
            {
                return;
            }
            foreach (var item in lines)
            {
                if (_stream.Position > 0)
                {
                    _stream.WriteByte(0x0A);
                }
                if (string.IsNullOrWhiteSpace(item))
                {
                    continue;
                }
                _stream.Write(Encoding.ASCII.GetBytes(item));
            }
        }

        public void Dispose()
        {
            WriteFooter();
            if (_options?.LeaveStreamOpen == false)
            {
                _stream.Dispose();
            }
        }
    }
}
