using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using UnityEngine;
using UnityEngine.Document;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Converters;

namespace ZoDream.BundleExtractor.Unity.Document
{
    internal class DocumentReader(ISerializedFile resource)
    {
        /// <summary>
        /// 根据类型声明获取内容
        /// </summary>
        /// <param name="m_Types"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public OrderedDictionary Read(VirtualDocument doc, IBundleBinaryReader reader)
        {
            reader.Position = 0; // 回到开始
            return ReadDict(doc, reader);
        }

        public void Read<T>(VirtualDocument doc, IBundleBinaryReader reader,
            T instance)
            where T : UnityEngine.Object
        {
            var data = Read(doc, reader);
            var obj = (object)instance;
            ConvertType(data, ref obj);
            data.Clear();
        }

        private void ConvertType(IDictionary data, ref object instance)
        {
            var type = instance.GetType();
            foreach (string key in data.Keys)
            {
                var fieldName = key.Trim().Replace(' ', '_');
                if (fieldName.StartsWith("m_"))
                {
                    fieldName = fieldName[2..];
                }
                fieldName = StringConverter.Studly(fieldName);
                if (instance is ResourceSource && fieldName == "Path")
                {
                    fieldName = "Source";
                }
                else if (fieldName == "Namespace")
                {
                    fieldName = "NameSpace";
                }
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

        public object ConvertType(object val, Type type)
        {
            if (val is IDictionary dict)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IPPtr<>))
                {
                    var pptr = (object)new PPtr();
                    ConvertType(dict, ref pptr);
                    return Activator.CreateInstance(
                        typeof(ObjectPPtr<>).MakeGenericType(type.GenericTypeArguments), [resource, pptr]);
                }
                var ctor = Activator.CreateInstance(type);
                if (ctor is null)
                {
                    return ctor;
                }
                ConvertType(dict, ref ctor);
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

        public T? ConvertType<T>(object val)
        {
            if (val is null)
            {
                return default;
            }
            return (T)ConvertType(val, typeof(T));
        }

        private object Read(VirtualNode node, IBundleBinaryReader reader)
        {
            var varTypeStr = node.Type;
            object value;
            var align = node.MetaFlag.IsAlignBytes();
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
                    break;
                case "map":
                    {
                        if (node.Children[0].MetaFlag.IsAlignBytes())
                        {
                            align = true;
                        }

                        int size = reader.ReadInt32();
                        var firstNode = node.Children[1].Children[0];
                        var secondNode = node.Children[1].Children[1];
                        value = reader.ReadArray(size, _ => new KeyValuePair<object, object>(Read(firstNode, reader),
                                Read(secondNode, reader)));
                        break;
                    }
                case "TypelessData":
                    {
                        int size = reader.ReadInt32();
                        value = reader.ReadBytes(size);
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
                            int size = reader.ReadInt32();
                            var itemNode = node.Children[0].Children[1];
                            value = reader.ReadArray(size, _ => Read(itemNode, reader));
                            break;
                        }
                        else //Class
                        {
                            value = ReadDict(node.Children, reader);
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

        private OrderedDictionary ReadDict(IEnumerable<VirtualNode> items, IBundleBinaryReader reader)
        {
            var res = new OrderedDictionary();
            foreach (var item in items)
            {
                res.Add(item.Name, Read(item, reader));
            }
            return res;
        }
    }
}
