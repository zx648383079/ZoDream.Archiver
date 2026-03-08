using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoDream.Shared.Bundle.Converters
{
    public class ArrayConverter : IBundleConverter
    {
        public static Type MakeType(Type itemIype) => itemIype.MakeArrayType();
        public bool CanConvert(Type objectType)
        {
            return objectType.IsArray;
        }

        public object? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var length = reader.ReadInt32();
            var itemType = objectType.GenericTypeArguments[0];
            var data = Array.CreateInstance(itemType, length);
            for (var i = 0; i < length; i++)
            {
                data.SetValue(serializer.Deserialize(reader, itemType), i);
            }
            return data;
        }
    }

    public class ListConverter : IBundleConverter
    {
        public static Type MakeType(Type itemIype) => typeof(List<>).MakeGenericType(itemIype);

        private readonly Type _baseType = typeof(IList<>);
        public bool CanConvert(Type objectType)
        {
            if (objectType.IsArray) 
            {
                return false;
            }
            if (objectType.IsInterface && objectType.GetGenericTypeDefinition() == _baseType)
            {
                return true;
            }
            foreach (var type in objectType.GetInterfaces())
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == _baseType)
                {
                    return true;
                }
            }
            return false;
        }

        public object? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var length = reader.ReadInt32();
            var itemType = objectType.GenericTypeArguments[0];
            var instanceType = objectType.IsClass ? objectType : typeof(List<>).MakeGenericType(objectType.GenericTypeArguments);
            var data = Activator.CreateInstance(instanceType);
            var addFn = _baseType.GetMethod("Add", objectType.GenericTypeArguments);
            for (var i = 0; i < length; i++)
            {
                addFn?.Invoke(data, [serializer.Deserialize(reader, itemType)]);
            }
            return data;
        }
    }

    public class MapConverter : IBundleConverter
    {
        public static Type MakeType(Type keyType, Type valueType) => typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
        private readonly Type _baseType = typeof(IDictionary<,>);
        public bool CanConvert(Type objectType)
        {
            if (objectType.IsArray)
            {
                return false;
            }
            if (objectType.IsInterface && objectType.GetGenericTypeDefinition() == _baseType)
            {
                return true;
            }
            foreach (var type in objectType.GetInterfaces())
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == _baseType)
                {
                    return true;
                }
            }
            return false;
        }

        public object? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var length = reader.ReadInt32();
            var instanceType = objectType.IsClass ? objectType : typeof(Dictionary<,>).MakeGenericType(objectType.GenericTypeArguments);
            var data = Activator.CreateInstance(instanceType);
            var addFn = _baseType.GetMethod("Add", objectType.GenericTypeArguments);
            for (var i = 0; i < length; i++)
            {
                addFn?.Invoke(data, [.. objectType.GenericTypeArguments.Select(i => serializer.Deserialize(reader, i))]);
            }
            return data;
        }
    }
}
