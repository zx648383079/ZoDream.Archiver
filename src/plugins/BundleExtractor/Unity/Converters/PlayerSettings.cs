using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class PlayerSettingsConverter : BundleConverter<PlayerSettings>
    {
        public override PlayerSettings? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            var version = reader.Get<Version>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
            }
            if (version.GreaterThanOrEquals(5, 4)) //5.4.0 nad up
            {
                var productGUID = reader.ReadBytes(16);
            }

            var AndroidProfiler = reader.ReadBoolean();

            //bool AndroidFilterTouchesWhenObscured 2017.2 and up
            //bool AndroidEnableSustainedPerformanceMode 2018 and up
            reader.AlignStream();
            int defaultScreenOrientation = reader.ReadInt32();
            int targetDevice = reader.ReadInt32();
            if (version.LessThan(5, 3)) //5.3 down
            {
                if (version.LessThan(5)) //5.0 down
                {
                    int targetPlatform = reader.ReadInt32(); //4.0 and up targetGlesGraphics
                    if (version.GreaterThanOrEquals(4, 6)) //4.6 and up
                    {
                        var targetIOSGraphics = reader.ReadInt32();
                    }
                }
                int targetResolution = reader.ReadInt32();
            }
            else
            {
                var useOnDemandResources = reader.ReadBoolean();
                reader.AlignStream();
            }
            if (version.GreaterThanOrEquals(3, 5)) //3.5 and up
            {
                var accelerometerFrequency = reader.ReadInt32();
            }
            
            return new()
            {
                CompanyName = reader.ReadAlignedString(),
                ProductName = reader.ReadAlignedString()
            };
        }
    }
}
