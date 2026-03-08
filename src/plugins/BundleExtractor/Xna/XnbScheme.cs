using System.IO;
using ZoDream.BundleExtractor.Converters;
using ZoDream.BundleExtractor.Xna.Converters;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Bundle.Converters;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;
using CharConverter = ZoDream.BundleExtractor.Xna.Converters.CharConverter;

namespace ZoDream.BundleExtractor.Xna
{
    public class XnbScheme(IEntryService service, IBundleOptions options) : IResourceScheme
    {
        internal static readonly IBundleConverter[] Converters = [
            new BooleanConverter(),
            new StringConverter(),
            new CharConverter(),
            new ByteConverter(),
            new SByteConverter(),
            new Int16Converter(),
            new UInt16Converter(),
            new Int32Converter(),
            new UInt32Converter(),
            new Int64Converter(),
            new UInt64Converter(),
            new SingleConverter(),
            new DoubleConverter(),
            new DecimalConverter(),

            new ArrayConverter(),
            new ListConverter(),
            new MapConverter(),

            new Vector2Converter(),
            new Vector3Converter(),
            new Vector4Converter(),
            new MatrixConverter(),
            new QuaternionConverter(),
            new Vector2IConverter(),
            new Vector3IConverter(),
            new Vector4IConverter(),
            new ColorConverter(),

            new StreamConverter(),

            new Texture2DConverter(),
            new Texture3DConverter(),
            new TextureCubeConverter(),
            new AnimatedTileConverter(),
            new DualTextureEffectConverter(),
            new EffectConverter(),
            new EffectMaterialConverter(),
            new EnvironmentMapEffectConverter(),
            new IndexBufferConverter(),
            new LayerConverter(),
            new BoundingBoxConverter(),
            new DateTimeConverter(),
            new PropertyConverter(),
            new RayConverter(),
            new SkinnedEffectConverter(),
            new SongConverter(),
            new SoundEffectConverter(),
            new SpriteFontConverter(),
            new StaticTileConverter(),
            new TBin10Converter(),
            new TileSheetConverter(),
            new VertexBufferConverter(),
            new VertexDeclarationConverter(),


            new FloatCurveConverter(),
            new VideoConverter(),


        ];

        public IBundleHandler? Open(string fileName)
        {
            if (!fileName.EndsWith(".xnb"))
            {
                return null;
            }
            return Open(File.OpenRead(fileName), fileName);
        }

        public IBundleHandler? Open(Stream stream, string fileName)
        {
            return new XnbReader(new BundleBinaryReader(stream, EndianType.LittleEndian, leaveOpen: false), service, options);
        }
    }
}
