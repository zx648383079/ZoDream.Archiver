﻿using System.Collections.Specialized;
using System.IO;
using System.Text.Json;
using UnityEngine;
using UnityEngine.Document;
using ZoDream.BundleExtractor.Unity.Document;
using ZoDream.BundleExtractor.Unity.Document.Cecil;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class BehaviorExporter(int entryId, ISerializedFile resource) : IBundleExporter
    {
        public string FileName => resource[entryId].Name;

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (resource[entryId] is not MonoBehaviour behavior)
            {
                return;
            }
            if (behavior.Script is null 
                || !behavior.Script.TryGet(out var script) 
                || script.ClassName.StartsWith("Cubism"))
            {
                return;
            }
            if (!LocationStorage.TryCreate(fileName, ".json", mode, out fileName))
            {
                return;
            }
            
            var data = Deserialize(entryId, resource);
            using var fs = File.Create(fileName);
            JsonSerializer.Serialize(fs, data, JsonExporter.Options);
        }



        public static VirtualDocument GetTypeNode(MonoBehaviour behavior, 
            IAssemblyReader assembly, IResourceEntry resource)
        {
            var builder = new DocumentBuilder(resource.Version);
            builder.AddMonoBehavior(0);
            if (behavior.Script.TryGet(out var script))
            {
                var typeDef = assembly.GetType(script.AssemblyName, 
                    string.IsNullOrEmpty(script.NameSpace) ? script.ClassName : $"{script.NameSpace}.{script.ClassName}");
                if (typeDef != null)
                {
                    var typeDefinitionConverter = new TypeDefinitionConverter(typeDef, builder, 1);
                    typeDefinitionConverter.ConvertTo();
                } else
                {
                    CubismExporter.ConvertToTypeTree(builder, script);
                }
            }
            return builder.ToDocument();
        }

        public static OrderedDictionary? Deserialize(int entryId, ISerializedFile resource)
        {
            var doc = resource.GetType(entryId);
            if (doc is null)
            {
                if (resource[entryId] is not MonoBehaviour behaviour)
                {
                    return null;
                }
                doc = GetTypeNode(behaviour, resource.Container.Assembly, resource);
            }
            return new DocumentReader(resource).Read(doc, resource.OpenRead(entryId));
        }

        public static OrderedDictionary? Deserialize(IPPtr<MonoBehaviour> ptr, 
            IAssemblyReader assembly, DocumentReader converter)
        {
            var doc = GetTypeNode(ptr, assembly);
            if (doc is null)
            {
                return null;
            }
            return converter.Read(doc, ptr.Resource.OpenRead(ptr.Index));
        }
        public static T? Deserialize<T>(IPPtr<MonoBehaviour> ptr, 
            IAssemblyReader assembly, DocumentReader converter)
            where T : MonoBehaviour, new()
        {
            var doc = GetTypeNode(ptr, assembly);
            if (doc is null)
            {
                return default;
            }
            var instance = new T();
            converter.Read(doc, ptr.Resource.OpenRead(ptr.Index), instance);
            return instance;
        }
        public static VirtualDocument GetTypeNode(IPPtr<MonoBehaviour> ptr, IAssemblyReader assembly)
        {
            var doc = ptr.Resource.GetType(ptr.Index);
            if (doc is null)
            {
                if (!ptr.TryGet(out var behaviour))
                {
                    return null;
                }
                doc = GetTypeNode(behaviour, assembly, ptr.Resource);
            }
            return doc;
        }


    }
}
