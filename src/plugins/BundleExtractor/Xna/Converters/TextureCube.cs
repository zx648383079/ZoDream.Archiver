using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class TextureCubeConverter : BundleConverter<TextureCube>
    {
        public override TextureCube? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var format = Texture2DConverter.ToFormat(reader.ReadInt32());
            var size = reader.ReadInt32();
            var count = reader.ReadInt32();
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    reader.ReadAsStream();
                }
            }
            return new TextureCube();
        }
    }
}
