using System;
using System.IO;
using System.Linq;
using UnityEngine.Document;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.BundleExtractor.Unity.Document;
using ZoDream.Shared.Bundle;
using ZoDream.SourceGenerator;

namespace ZoDream.BundleExtractor.Unity
{
    public class TypeTreeSerializer(VirtualDocument doc) : IBundleSerializer
    {
        public IBundleConverterCollection Converters { get; set; } = new BundleConverterCollection(UnityConverter.Converters);


        public VirtualDocument Get(Type objectType)
        {
            var maps = doc.Children.Where(i => i.Type == objectType.Name).FirstOrDefault();
            return new VirtualDocument(doc.Version, maps is null ? [] : [maps]);
        }

        public T? Deserialize<T>(IBundleBinaryReader reader)
        {
            return (T?)Deserialize(reader, typeof(T));
        }

        public object? Deserialize(IBundleBinaryReader reader, Type objectType)
        {
            if (!reader.TryGet<ISerializedFile>(out var resource))
            {
                return null;
            }
            var maps = doc.Children.Where(i => i.Type == objectType.Name).FirstOrDefault();
            if (maps is null)
            {
                return null;
            }
            if (Converters.TryGet(objectType, out var converter) && converter is ITypeTreeConverter tl)
            {
                // 一些需要读取数据后进行预处理的
                return tl.Read(reader, objectType, new VirtualDocument(doc.Version, [maps]));
            }
            var instance = Activator.CreateInstance(objectType)!;
            new DocumentReader(resource).ReadObject(ref instance, maps.Children, reader);
            return instance;
        }

        public static IBundleSerializer CreateForm(string fileName)
        {
            using var fs = File.OpenRead(fileName);
            return new TypeTreeSerializer(new TypeNodeReader(fs).Read());
        }
    }
}
