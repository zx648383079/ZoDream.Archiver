using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SpriteSettingsConverter : BundleConverter<SpriteSettings>
    {
        public override SpriteSettings? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new SpriteSettings
            {
                SettingsRaw = reader.ReadUInt32()
            };

            res.Packed = res.SettingsRaw & 1; //1
            res.PackingMode = (SpritePackingMode)(res.SettingsRaw >> 1 & 1); //1
            res.PackingRotation = (SpritePackingRotation)(res.SettingsRaw >> 2 & 0xf); //4
            res.MeshType = (SpriteMeshType)(res.SettingsRaw >> 6 & 1); //1
            //reserved
            return res;
        }
    }

}
