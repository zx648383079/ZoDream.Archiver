using System.IO;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class TuanJieElementScanner(string package, IBundleOptions options) : 
        IBundleElementScanner, IBundleStorage,
        IBundleCodec
    {
        public bool IsFakeHeader => package.Contains("fake");
        public bool IsGuiLongChao => package.Contains("glc");

        public Stream Open(string fullPath)
        {
            return File.OpenRead(fullPath);
        }

        public IBundleBinaryReader OpenRead(string fullPath)
        {
            return OpenRead(Open(fullPath), fullPath);
        }

        public IBundleBinaryReader OpenRead(Stream input, string fileName)
        {
            if (IsFakeHeader && !FileNameHelper.IsCommonFile(fileName))
            {
                input = OtherBundleElementScanner.ParseFakeHeader(input);
            }
            return new BundleBinaryReader(input, EndianType.BigEndian);
        }

        public bool TryRead(IBundleBinaryReader reader, object instance)
        {
            if (instance is Mesh m)
            {
                CreateInstance(reader, m);
                return true;
            }
            if (instance is ConstantBuffer cb)
            {
                CreateInstance(reader, cb);
                return true;
            }
            if (instance is MatrixParameter mp)
            {
                CreateInstance(reader, mp);
                return true;
            }
            if (instance is VectorParameter vp)
            {
                CreateInstance(reader, vp);
                return true;
            }
            if (instance is AnimationClip ac)
            {
                CreateInstance(reader, ac);
                return true;
            }
            if (instance is Texture2D t)
            {
                CreateInstance(reader, t);
                return true;
            }
            if (instance is GameObject g)
            {
                CreateInstance(reader, g);
                return true;
            }
            if (instance is IElementLoader l)
            {
                l.Read(reader);
                return true;
            }
            return false;
        }
    }
}
