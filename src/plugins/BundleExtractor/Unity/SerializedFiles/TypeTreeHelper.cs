﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.SerializedFiles
{
    internal static class TypeTreeHelper
    {
        public static string ReadTypeString(TypeTree m_Type, 
            IBundleBinaryReader reader)
        {
            var sb = new StringBuilder();
            List<TypeTreeNode>? m_Nodes = m_Type.Nodes;
            for (int i = 0; i < m_Nodes.Count; i++)
            {
                ReadStringValue(sb, m_Nodes, reader, ref i);
            }
            return sb.ToString();
        }

        private static void ReadStringValue(StringBuilder sb, 
            List<TypeTreeNode> m_Nodes,
            IBundleBinaryReader reader, 
            ref int i)
        {
            TypeTreeNode? m_Node = m_Nodes[i];
            byte level = m_Node.Level;
            string? varTypeStr = m_Node.Type;
            string? varNameStr = m_Node.Name;
            object? value = null;
            bool append = true;
            bool align = m_Node.MetaFlag.IsAlignBytes();
            switch (varTypeStr)
            {
                case "SInt8":
                    value = reader.ReadSByte();
                    break;
                case "UInt8":
                case "char":
                    value = reader.ReadByte();
                    break;
                case "short":
                case "SInt16":
                    value = reader.ReadInt16();
                    break;
                case "UInt16":
                case "unsigned short":
                    value = reader.ReadUInt16();
                    break;
                case "int":
                case "SInt32":
                    value = reader.ReadInt32();
                    break;
                case "UInt32":
                case "unsigned int":
                case "Type*":
                    value = reader.ReadUInt32();
                    break;
                case "long long":
                case "SInt64":
                    value = reader.ReadInt64();
                    break;
                case "UInt64":
                case "unsigned long long":
                case "FileSize":
                    value = reader.ReadUInt64();
                    break;
                case "float":
                    value = reader.ReadSingle();
                    break;
                case "double":
                    value = reader.ReadDouble();
                    break;
                case "bool":
                    value = reader.ReadBoolean();
                    break;
                case "string":
                    append = false;
                    string? str = reader.ReadAlignedString();
                    sb.AppendFormat("{0}{1} {2} = \"{3}\"\r\n", new string('\t', level), varTypeStr, varNameStr, str);
                    i += 3;
                    break;
                case "map":
                    {
                        if (m_Nodes[i + 1].MetaFlag.IsAlignBytes())
                        {
                            align = true;
                        }

                        append = false;
                        sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level), varTypeStr, varNameStr);
                        sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level + 1), "Array", "Array");
                        int size = reader.ReadInt32();
                        sb.AppendFormat("{0}{1} {2} = {3}\r\n", new string('\t', level + 1), "int", "size", size);
                        List<TypeTreeNode>? map = GetNodes(m_Nodes, i);
                        i += map.Count - 1;
                        List<TypeTreeNode>? first = GetNodes(map, 4);
                        int next = 4 + first.Count;
                        List<TypeTreeNode>? second = GetNodes(map, next);
                        for (int j = 0; j < size; j++)
                        {
                            sb.AppendFormat("{0}[{1}]\r\n", new string('\t', level + 2), j);
                            sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level + 2), "pair", "data");
                            int tmp1 = 0;
                            int tmp2 = 0;
                            ReadStringValue(sb, first, reader, ref tmp1);
                            ReadStringValue(sb, second, reader, ref tmp2);
                        }
                        break;
                    }
                case "TypelessData":
                    {
                        append = false;
                        int size = reader.ReadInt32();
                        reader.ReadBytes(size);
                        i += 2;
                        sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level), varTypeStr, varNameStr);
                        sb.AppendFormat("{0}{1} {2} = {3}\r\n", new string('\t', level), "int", "size", size);
                        break;
                    }
                default:
                    {
                        if (i < m_Nodes.Count - 1 && m_Nodes[i + 1].Type == "Array") //Array
                        {
                            if (m_Nodes[i + 1].MetaFlag.IsAlignBytes())
                            {
                                align = true;
                            }

                            append = false;
                            sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level), varTypeStr, varNameStr);
                            sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level + 1), "Array", "Array");
                            int size = reader.ReadInt32();
                            sb.AppendFormat("{0}{1} {2} = {3}\r\n", new string('\t', level + 1), "int", "size", size);
                            List<TypeTreeNode>? vector = GetNodes(m_Nodes, i);
                            i += vector.Count - 1;
                            for (int j = 0; j < size; j++)
                            {
                                sb.AppendFormat("{0}[{1}]\r\n", new string('\t', level + 2), j);
                                int tmp = 3;
                                ReadStringValue(sb, vector, reader, ref tmp);
                            }
                            break;
                        }
                        else //Class
                        {
                            append = false;
                            sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level), varTypeStr, varNameStr);
                            List<TypeTreeNode>? @class = GetNodes(m_Nodes, i);
                            i += @class.Count - 1;
                            for (int j = 1; j < @class.Count; j++)
                            {
                                ReadStringValue(sb, @class, reader, ref j);
                            }
                            break;
                        }
                    }
            }
            if (append)
            {
                sb.AppendFormat("{0}{1} {2} = {3}\r\n", new string('\t', level), varTypeStr, varNameStr, value);
            }

            if (align)
            {
                reader.AlignStream();
            }
        }
        /// <summary>
        /// 根据类型声明获取内容
        /// </summary>
        /// <param name="m_Types"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static OrderedDictionary ReadType(TypeTree m_Types, 
            IBundleBinaryReader reader)
        {
            reader.Position = 0; // 回到开始
            var obj = new OrderedDictionary();
            var m_Nodes = m_Types.Nodes;
            for (int i = 1; i < m_Nodes.Count; i++)
            {
                var m_Node = m_Nodes[i];
                var varNameStr = m_Node.Name;
                obj[varNameStr] = ReadValue(m_Nodes, reader, ref i);
            }
            return obj;
        }

        public static void ReadType<T>(TypeTree m_Types, 
            IBundleBinaryReader reader, 
            T instance)
            where T : UIObject 
        {
            var data = ReadType(m_Types, reader);
            ConvertType(data, instance);
            data.Clear();
        }

        private static void ConvertType(IDictionary data, object instance)
        {
            var type = instance.GetType();
            foreach (string key in data.Keys)
            {
                var fieldName = key.Trim().Replace(' ', '_');
                var obj = data[key];
                if (obj is null)
                {
                    continue;
                }
                var field = type.GetField(fieldName);
                if (field is not null)
                {
                    field?.SetValue(instance, ConvertType(obj, field.FieldType));
                    continue;
                }
                var property = type.GetProperty(fieldName);
                if (property is not null)
                {
                    property?.SetValue(instance, ConvertType(obj, property.PropertyType));
                }
            }
        }

        private static object ConvertType(object val, Type type)
        {
            if (val is IDictionary dict)
            {
                var ctor = Activator.CreateInstance(type);
                if (ctor is null)
                {
                    return ctor;
                }
                ConvertType(dict, ctor);
                return ctor;
            }
            if (type == typeof(Stream))
            {
                if (val is byte[] buffer && buffer.Length > 0)
                {
                    return new MemoryStream(buffer);
                }
                return null;
            }
            if (type.IsEnum)
            {
                return Enum.ToObject(type, val);
            }
            return Convert.ChangeType(val, type);
        }

        private static object ReadValue(List<TypeTreeNode> m_Nodes, 
            IBundleBinaryReader reader, ref int i)
        {
            var m_Node = m_Nodes[i];
            var varTypeStr = m_Node.Type;
            object value;
            var align = m_Node.MetaFlag.IsAlignBytes();
            switch (varTypeStr)
            {
                case "SInt8":
                    value = reader.ReadSByte();
                    break;
                case "UInt8":
                case "char":
                    value = reader.ReadByte();
                    break;
                case "short":
                case "SInt16":
                    value = reader.ReadInt16();
                    break;
                case "UInt16":
                case "unsigned short":
                    value = reader.ReadUInt16();
                    break;
                case "int":
                case "SInt32":
                    value = reader.ReadInt32();
                    break;
                case "UInt32":
                case "unsigned int":
                case "Type*":
                    value = reader.ReadUInt32();
                    break;
                case "long long":
                case "SInt64":
                    value = reader.ReadInt64();
                    break;
                case "UInt64":
                case "unsigned long long":
                case "FileSize":
                    value = reader.ReadUInt64();
                    break;
                case "float":
                    value = reader.ReadSingle();
                    break;
                case "double":
                    value = reader.ReadDouble();
                    break;
                case "bool":
                    value = reader.ReadBoolean();
                    break;
                case "string":
                    value = reader.ReadAlignedString();
                    i += 3;
                    break;
                case "map":
                    {
                        if (m_Nodes[i + 1].MetaFlag.IsAlignBytes())
                        {
                            align = true;
                        }

                        var map = GetNodes(m_Nodes, i);
                        i += map.Count - 1;
                        var first = GetNodes(map, 4);
                        int next = 4 + first.Count;
                        var second = GetNodes(map, next);
                        int size = reader.ReadInt32();
                        var dic = new List<KeyValuePair<object, object>>(size);
                        for (int j = 0; j < size; j++)
                        {
                            int tmp1 = 0;
                            int tmp2 = 0;
                            dic.Add(new KeyValuePair<object, object>(ReadValue(first, reader, ref tmp1), ReadValue(second, reader, ref tmp2)));
                        }
                        value = dic;
                        break;
                    }
                case "TypelessData":
                    {
                        int size = reader.ReadInt32();
                        value = reader.ReadBytes(size);
                        i += 2;
                        break;
                    }
                default:
                    {
                        if (i < m_Nodes.Count - 1 && m_Nodes[i + 1].Type == "Array") //Array
                        {
                            if (m_Nodes[i + 1].MetaFlag.IsAlignBytes())
                            {
                                align = true;
                            }

                            var vector = GetNodes(m_Nodes, i);
                            i += vector.Count - 1;
                            int size = reader.ReadInt32();
                            var list = new List<object>(size);
                            for (int j = 0; j < size; j++)
                            {
                                int tmp = 3;
                                list.Add(ReadValue(vector, reader, ref tmp));
                            }
                            value = list;
                            break;
                        }
                        else //Class
                        {
                            var cls = GetNodes(m_Nodes, i);
                            i += cls.Count - 1;
                            var obj = new OrderedDictionary();
                            for (int j = 1; j < cls.Count; j++)
                            {
                                var classMember = cls[j];
                                var name = classMember.Name;
                                obj[name] = ReadValue(cls, reader, ref j);
                            }
                            value = obj;
                            break;
                        }
                    }
            }
            if (align)
            {
                reader.AlignStream();
            }

            return value;
        }

        private static List<TypeTreeNode> GetNodes(List<TypeTreeNode> m_Nodes, int index)
        {
            var nodes = new List<TypeTreeNode>
            {
                m_Nodes[index]
            };
            var level = m_Nodes[index].Level;
            for (var i = index + 1; i < m_Nodes.Count; i++)
            {
                var member = m_Nodes[i];
                var level2 = member.Level;
                if (level2 <= level)
                {
                    return nodes;
                }
                nodes.Add(member);
            }
            return nodes;
        }
    }
}
