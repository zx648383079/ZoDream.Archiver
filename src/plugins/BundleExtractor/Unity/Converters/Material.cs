using System;
using UnityEngine;
using UnityEngine.Document;
using ZoDream.BundleExtractor.Unity.Document;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class MaterialConverter : BundleConverter<Material>, IElementTypeLoader
    {
        public static void ReadBase(Material res, IBundleBinaryReader reader, 
            IBundleSerializer serializer, Action cb)
        {
            var target = reader.Get<BuildTarget>();
            var version = reader.Get<Version>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            res.Name = reader.ReadAlignedString();
            res.Shader = reader.ReadPPtr<Shader>(serializer);

            if (version.Major == 4 && version.Minor >= 1) //4.x
            {
                var m_ShaderKeywords = reader.ReadArray(r => r.ReadAlignedString());
            }

            if (version.GreaterThanOrEquals(2021, 3)) //2021.3 and up
            {
                var m_ValidKeywords = reader.ReadArray(r => r.ReadAlignedString());
                var m_InvalidKeywords = reader.ReadArray(r => r.ReadAlignedString());
            }
            else if (version.GreaterThanOrEquals(5)) //5.0 ~ 2021.2
            {
                var m_ShaderKeywords = reader.ReadAlignedString();
            }

            if (version.GreaterThanOrEquals(5)) //5.0 and up
            {
                var m_LightmapFlags = reader.ReadUInt32();
            }

            if (version.GreaterThanOrEquals(5, 6)) //5.6 and up
            {
                var m_EnableInstancingVariants = reader.ReadBoolean();
                //var m_DoubleSidedGI = a_Stream.ReadBoolean(); //2017 and up
                reader.AlignStream();
            }
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                var m_CustomRenderQueue = reader.ReadInt32();
            }
            cb.Invoke();
        }

        public override Material? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new Material();
            ReadBase(res, reader, serializer, () => { });
            var version = reader.Get<Version>();

            if (version.GreaterThanOrEquals(5, 1)) //5.1 and up
            {
                var stringTagMapSize = reader.ReadInt32();
                for (int i = 0; i < stringTagMapSize; i++)
                {
                    var first = reader.ReadAlignedString();
                    var second = reader.ReadAlignedString();
                }
            }


            if (version.GreaterThanOrEquals(5, 6)) //5.6 and up
            {
                var disabledShaderPasses = reader.ReadArray(r => r.ReadAlignedString());
            }

            res.SavedProperties = serializer.Deserialize<PropertySheet>(reader);

            //vector m_BuildTextureStacks 2020 and up
            return res;
        }

        public object? Read(IBundleBinaryReader reader, Type target, VirtualDocument typeMaps)
        {
            var res = new Material();
            var container = reader.Get<ISerializedFile>();
            new DocumentReader(container).Read(typeMaps, reader, res);
            return res;
        }
    }
}
