using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZoDream.BundleExtractor.Unity.UI
{
    public sealed class PlayerSettings : UIObject
    {
        public string companyName;
        public string productName;

        public PlayerSettings(UIReader reader) : base(reader)
        {
            var version = reader.Version;
            if (version.GreaterThanOrEquals(5, 4)) //5.4.0 nad up
            {
                var productGUID = reader.Reader.ReadBytes(16);
            }

            var AndroidProfiler = reader.Reader.ReadBoolean();
            //bool AndroidFilterTouchesWhenObscured 2017.2 and up
            //bool AndroidEnableSustainedPerformanceMode 2018 and up
            reader.Reader.AlignStream();
            int defaultScreenOrientation = reader.Reader.ReadInt32();
            int targetDevice = reader.Reader.ReadInt32();
            if (version.LessThan(5, 3)) //5.3 down
            {
                if (version.LessThan(5)) //5.0 down
                {
                    int targetPlatform = reader.Reader.ReadInt32(); //4.0 and up targetGlesGraphics
                    if (version.GreaterThanOrEquals(4, 6)) //4.6 and up
                    {
                        var targetIOSGraphics = reader.Reader.ReadInt32();
                    }
                }
                int targetResolution = reader.Reader.ReadInt32();
            }
            else
            {
                var useOnDemandResources = reader.Reader.ReadBoolean();
                reader.Reader.AlignStream();
            }
            if (version.GreaterThanOrEquals(3, 5)) //3.5 and up
            {
                var accelerometerFrequency = reader.Reader.ReadInt32();
            }
            companyName = reader.ReadAlignedString();
            productName = reader.ReadAlignedString();
        }
    }
}
