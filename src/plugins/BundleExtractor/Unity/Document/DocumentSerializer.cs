using System.Linq;
using UnityEngine.Document;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Language;

namespace ZoDream.BundleExtractor.Unity.Document
{
    public class DocumentSerializer
    {
        public string Serialize(VirtualDocument doc, IBundleBinaryReader reader)
        {
            var writer = new CodeWriter();
            foreach (var item in doc)
            {
                Serialize(writer, item, reader);
            }
            return writer.ToString() ?? string.Empty;
        }

        private static void Serialize(CodeWriter writer, VirtualNode node, IBundleBinaryReader reader)
        {
            var align = node.MetaFlag.IsAlignBytes();
            switch (node.Type)
            {
                case "SInt8":
                    writer.WriteFormat("{0} {1} = {2}", node.Type, node.Name, reader.ReadSByte()).WriteLine(true);
                    break;
                case "UInt8":
                case "char":
                    writer.WriteFormat("{0} {1} = {2}", node.Type, node.Name, reader.ReadByte()).WriteLine(true);
                    break;
                case "short":
                case "SInt16":
                    writer.WriteFormat("{0} {1} = {2}", node.Type, node.Name, reader.ReadInt16()).WriteLine(true);
                    break;
                case "UInt16":
                case "unsigned short":
                    writer.WriteFormat("{0} {1} = {2}", node.Type, node.Name, reader.ReadUInt16()).WriteLine(true);
                    break;
                case "int":
                case "SInt32":
                    writer.WriteFormat("{0} {1} = {2}", node.Type, node.Name, reader.ReadInt32()).WriteLine(true);
                    break;
                case "UInt32":
                case "unsigned int":
                case "Type*":
                    writer.WriteFormat("{0} {1} = {2}", node.Type, node.Name, reader.ReadUInt32()).WriteLine(true);
                    break;
                case "long long":
                case "SInt64":
                    writer.WriteFormat("{0} {1} = {2}", node.Type, node.Name, reader.ReadInt64()).WriteLine(true);
                    break;
                case "UInt64":
                case "unsigned long long":
                case "FileSize":
                    writer.WriteFormat("{0} {1} = {2}", node.Type, node.Name, reader.ReadUInt64()).WriteLine(true);
                    break;
                case "float":
                    writer.WriteFormat("{0} {1} = {2}", node.Type, node.Name, reader.ReadSingle()).WriteLine(true);
                    break;
                case "double":
                    writer.WriteFormat("{0} {1} = {2}", node.Type, node.Name, reader.ReadDouble()).WriteLine(true);
                    break;
                case "bool":
                    writer.WriteFormat("{0} {1} = {2}", node.Type, node.Name, reader.ReadBoolean()).WriteLine(true);
                    break;
                case "string":
                    writer.WriteFormat("{0} {1} = \"{2}\"", node.Type, node.Name, reader.ReadAlignedString()).WriteLine(true);
                    break;
                case "map":
                    {
                        if (node.Children[0].MetaFlag.IsAlignBytes())
                        {
                            align = true;
                        }
                        var size = reader.ReadInt32();
                        writer.WriteFormat("{0} {1} = {2}", node.Type, node.Name)
                            .WriteIndentLine()
                            .Write("Array Array")
                            .WriteLine(true)
                            .WriteFormat("int size = {0}", size)
                            .WriteIndentLine();
                        for (int j = 0; j < size; j++)
                        {
                            writer.WriteFormat("[{0}]", j)
                                .WriteLine(true)
                                .Write("pair data")
                                .WriteLine(true);
                            foreach (var item in node.Children[1].Children)
                            {
                                Serialize(writer, item, reader);
                            }
                        }
                        writer.Indent -= 2;
                        writer.WriteLine(true);
                        break;
                    }
                case "TypelessData":
                    {
                        var size = reader.ReadInt32();
                        reader.Position += size;
                        writer.WriteFormat("{0} {1}", node.Type, node.Name)
                            .WriteLine(true)
                            .WriteFormat("int size = {0}", size).WriteLine(true);
                        break;
                    }
                default:
                    {
                        if (node.Children.Length > 0 && node.Children[0].Type == "Array") //Array
                        {
                            if (node.Children[0].MetaFlag.IsAlignBytes())
                            {
                                align = true;
                            }
                            var size = reader.ReadInt32();
                            writer.WriteFormat("{0} {1}", node.Type, node.Name)
                                .WriteIndentLine()
                                .Write("Array Array")
                                .WriteLine(true)
                                .WriteFormat("int size = {0}", size)
                                .WriteIndentLine();

                            foreach (var item in node.Children[0].Children.Skip(1))
                            {
                                Serialize(writer, item, reader);
                            }
                            writer.WriteOutdentLine();
                            break;
                        }
                        else //Class
                        {
                            writer.WriteFormat("{0} {1}", node.Type, node.Name)
                                .WriteLine(true);
                            foreach (var item in node.Children)
                            {
                                Serialize(writer, item, reader);
                            }
                            break;
                        }
                    }
            }

            if (align)
            {
                reader.AlignStream();
            }
        }

    }
}
