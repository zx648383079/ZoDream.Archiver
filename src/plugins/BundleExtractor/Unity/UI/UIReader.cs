using System;
using System.IO;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Producers;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class UIReader: BundleBinaryReader
    {
        public UIReader(IBundleBinaryReader reader,
                ObjectInfo data,
                ISerializedFile source,
                IBundleOptions options)
            : base(reader.BaseStream, reader.EndianType, reader.IsAlignStream)
        {
            Add(Data = data);
            Add(Source = source);
            Add(Options = options);
            Add(Source.Version);
            Add(Source.UnityVersion);
            Add(SerializedType);
        }

        public ObjectInfo Data { get; private set; }
        public ISerializedFile Source { get; private set; }
        public IBundleOptions Options { get; private set; }

        public ElementIDType Type
        {
            get
            {
                if (Enum.IsDefined(typeof(ElementIDType), (int)Data.ClassID))
                {
                    return (ElementIDType)Data.ClassID;
                }
                else
                {
                    return ElementIDType.UnknownType;
                }
            }
        }

        public string FullPath => Source.FullPath;

        public SerializedType SerializedType => Source.TypeItems[Data.SerializedTypeIndex];
        public UnityVersion Version => Source.UnityVersion;
        public BuildTarget Platform => Source.Platform;


        public Stream OpenResource(string fileName)
        {
            return Source.Container!.OpenResource(fileName, Source);
        }

        public Stream? OpenResource(StreamingInfo info)
        {
            if (string.IsNullOrWhiteSpace(info.path))
            {
                return null;
            }
            var stream = OpenResource(info.path);
            if (stream is null)
            {
                return null;
            }
            return new PartialStream(stream, info.offset, info.size);
        }

    }
}
