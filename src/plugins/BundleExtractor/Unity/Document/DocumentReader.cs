using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEngine;
using UnityEngine.Document;
using ZoDream.Shared;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Converters;
using Object = UnityEngine.Object;

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
            if (doc.Children.Length == 0)
            {
                return [];
            }
            return ReadDict(doc.Children[0].Children, reader);
        }
        /// <summary>
        /// 根据根节点的类型自动创建
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public object? ReadAs(VirtualDocument doc, IBundleBinaryReader reader)
        {
            if (doc.Children.Length != 1)
            {
                return null;
            }
            reader.Position = 0; // 回到开始
            var objType = typeof(Object);
            var objNamespace = objType.Namespace;
            var rootType = objType.Assembly.GetType($"{objNamespace}.{doc.Children[0].Type}");
            if (rootType is null || !objType.IsAssignableFrom(rootType))
            {
                return null;
            }
            if (Activator.CreateInstance(rootType) is not Object instance)
            {
                return null;
            }
            Read(doc, reader, instance);
            return instance;
        }

        public void Read<T>(VirtualDocument doc, IBundleBinaryReader reader,
            T instance)
            where T : Object
        {
            var obj = (object)instance;
            ReadObject(ref obj, doc.Children[0].Children, reader);
        }

        internal void ReadObject(ref object instance, VirtualNode[] nodes, IBundleBinaryReader reader)
        {
            var type = instance.GetType();
            foreach (var node in nodes)
            {
                var fieldName = ConvertFieldName(node.Name, instance);
                var field = type.GetField(fieldName);
                if (field is not null)
                {
                    field?.SetValue(instance, ReadType(field.FieldType, node, reader));
                    continue;
                }
                var property = type.GetProperty(fieldName);
                if (property is not null)
                {
                    property?.SetValue(instance, ReadType(property.PropertyType, node, reader));
                    continue;
                }
                Read(node, reader);
            }
        }

        private object? ReadType(Type type, VirtualNode node, IBundleBinaryReader reader)
        {
            var align = node.MetaFlag.IsAlignBytes();
            if (TryRead(node, reader, out var value))
            {
                if (align)
                {
                    reader.AlignStream();
                }
                return ConvertType(value, type);
            }
            var isFromType = false;
            switch (node.Type)
            {
                case "map":
                    {
                        if (node.Children[0].MetaFlag.IsAlignBytes())
                        {
                            align = true;
                        }
                        int size = reader.ReadInt32();
                        var childNode = node.Children[0].Children[1].Children;
                        Expectation.ThrowIfNot(childNode.Length == 2);
                        Type childType = null;
                        var inType = typeof(IEnumerable<>);
                        foreach (var item in type.GetInterfaces())
                        {
                            if (item.IsGenericType && item.GetGenericTypeDefinition() == inType)
                            {
                                childType = item.GenericTypeArguments[0];
                                break;
                            }
                        }
                        Expectation.ThrowIfNot(childType is not null);
                        value = ReadArray(childType, size, () =>
                                Activator.CreateInstance(childType, 
                                    ReadType(childType.GenericTypeArguments[0], childNode[0], reader),
                                    ReadType(childType.GenericTypeArguments[1], childNode[1], reader)));
                        if (!type.IsArray)
                        {
                            value = Activator.CreateInstance(type, [Convert.ChangeType(value, childType)]);
                        }
                        isFromType = true;
                        break;
                    }
                case "TypelessData":
                    {
                        var size = reader.ReadInt32();
                        if (type == typeof(Stream))
                        {
                            value = reader.ReadAsStream(size);
                        } else if (type == typeof(byte[]))
                        {
                            value = reader.ReadBytes(size);
                        } else
                        {
                            value = null;
                            reader.Position += size;
                        }
                        isFromType = true;
                        break;
                    }
                default:
                    {
                        if (node.Children.Length == 1 && node.Children[0].Type == "Array") //Array
                        {
                            if (node.Children[0].MetaFlag.IsAlignBytes())
                            {
                                align = true;
                            }
                            int size = reader.ReadInt32();
                            var itemNode = node.Children[0].Children[1];
                            var itemType = type.IsArray ? type.GetElementType() : type.GenericTypeArguments[0];
                            value = ReadArray(itemType, size, () => ReadType(itemType, itemNode, reader));
                            if (!type.IsArray)
                            {
                                value = Activator.CreateInstance(type, [value]);
                            }
                            isFromType = true;
                        }
                        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IPPtr<>))
                        {
                            var pptr = (object)new PPtr();
                            ReadObject(ref pptr, node.Children, reader);
                            return Activator.CreateInstance(
                                typeof(ObjectPPtr<>).MakeGenericType(type.GenericTypeArguments), [resource, pptr]);
                        }
                        else //Class
                        {
                            value = Activator.CreateInstance(type);
                            ReadObject(ref value, node.Children, reader);
                            isFromType = true;
                        }
                        break;
                    }
            }
            if (align)
            {
                reader.AlignStream();
            }
            if (isFromType)
            {
                return value;
            }
            return ConvertType(value, type);
        }

        private static Array ReadArray(Type itemType, int length, Func<object?> cb)
        {
            var data = Array.CreateInstance(itemType, length);
            for (int i = 0; i < length; i++)
            {
                data.SetValue(cb.Invoke(), i);
            }
            return data;
        }

        private static object ReadList(Type itemType, int length, Func<object?> cb)
        {
            var type = typeof(List<>).MakeGenericType(itemType);
            var addFn = type.GetMethod("Add", [itemType]);
            var data = Activator.CreateInstance(type, length);
            for (int i = 0; i < length; i++)
            {
                addFn?.Invoke(data, [cb.Invoke()]);
            }
            return data;
        }


        private void ConvertType(IDictionary data, ref object instance)
        {
            var type = instance.GetType();
            foreach (string key in data.Keys)
            {
                var fieldName = ConvertFieldName(key, instance);
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
            var align = node.MetaFlag.IsAlignBytes();
            if (TryRead(node, reader, out var value))
            {
                if (align)
                {
                    reader.AlignStream();
                }
                return value;
            }
            switch (node.Type)
            {
                case "map":// map.Array.pair
                    {
                        if (node.Children[0].MetaFlag.IsAlignBytes())
                        {
                            align = true;
                        }
                        int size = reader.ReadInt32();
                        var childNode = node.Children[0].Children[1].Children;
                        value = reader.ReadArray(size, _ => 
                        new KeyValuePair<object, object>(
                            Read(childNode[0], reader),
                                Read(childNode[1], reader)));
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

        private static bool TryRead(VirtualNode node, IBundleBinaryReader reader, [NotNullWhen(true)] out object? result)
        {
            switch (node.Type)
            {
                case "SInt8":
                    result = reader.ReadSByte();
                    return true;
                case "UInt8":
                case "char":
                    result = reader.ReadByte();
                    return true;
                case "short":
                case "SInt16":
                    result = reader.ReadInt16();
                    return true;
                case "UInt16":
                case "unsigned short":
                    result = reader.ReadUInt16();
                    return true;
                case "int":
                case "SInt32":
                    result = reader.ReadInt32();
                    return true;
                case "UInt32":
                case "unsigned int":
                case "Type*":
                    result = reader.ReadUInt32();
                    return true;
                case "long long":
                case "SInt64":
                    result = reader.ReadInt64();
                    return true;
                case "UInt64":
                case "unsigned long long":
                case "FileSize":
                    result = reader.ReadUInt64();
                    return true;
                case "float":
                    result = reader.ReadSingle();
                    return true;
                case "double":
                    result = reader.ReadDouble();
                    return true;
                case "bool":
                    result = reader.ReadBoolean();
                    return true;
                case "string":
                    result = reader.ReadAlignedString();
                    return true;
                default:
                    result = null;
                    return false;
            }
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
    
        private static Type ConvertType(VirtualNode node)
        {
            switch (node.Type)
            {
                case "SInt8":
                    return typeof(sbyte);
                case "UInt8":
                case "char":
                    return typeof(byte);
                case "short":
                case "SInt16":
                    return typeof(short);
                case "UInt16":
                case "unsigned short":
                    return typeof(ushort);
                case "int":
                case "SInt32":
                    return typeof(int);
                case "UInt32":
                case "unsigned int":
                case "Type*":
                    return typeof(uint);
                case "long long":
                case "SInt64":
                    return typeof(long);
                case "UInt64":
                case "unsigned long long":
                case "FileSize":
                    return typeof(ulong);
                case "float":
                    return typeof(float);
                case "double":
                    return typeof(double);
                case "bool":
                    return typeof(bool);
                case "string":
                    return typeof(string);
                case "map":
                    return typeof(Array);//.MakeGenericType(typeof(KeyValuePair<,>)
                        // .MakeGenericType([.. node.Children[0].Children[1].Children.Select(ConvertType)]));
                case "TypelessData":
                    return typeof(byte[]);
                default:
                    if (node.Children.Length > 0 && node.Children[0].Type == "Array")
                    {
                        return typeof(Array);//.MakeGenericType(ConvertType(node.Children[0].Children[1]));
                    }
                    return typeof(OrderedDictionary);
            }
        }

        private static string ConvertFieldName(string key, object host)
        {
            var fieldName = key.Trim().Replace(' ', '_');
            if (fieldName.StartsWith("m_"))
            {
                fieldName = fieldName[2..];
            }
            fieldName = StringConverter.Studly(fieldName);
            if (host is ResourceSource && fieldName == "Path")
            {
                fieldName = "Source";
            }
            else if (fieldName == "Namespace")
            {
                fieldName = "NameSpace";
            }
            return fieldName;
        }
    }
}
