using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity
{
    public enum UnityVersionType : byte
    {
        Alpha = 0,
        //
        // 摘要:
        //     b
        Beta = 1,
        //
        // 摘要:
        //     c
        China = 2,
        //
        // 摘要:
        //     f
        Final = 3,
        //
        // 摘要:
        //     p
        Patch = 4,
        //
        // 摘要:
        //     x
        Experimental = 5,
        //
        // 摘要:
        //     The minimum valid value for this enumeration
        MinValue = 0,
        //
        // 摘要:
        //     The maximum valid value for this enumeration
        MaxValue = 5
    }
    public static class UnityVersionTypeExtentions
    {
        public static char ToCharacter(this UnityVersionType type)
        {
            return type switch
            {
                UnityVersionType.Alpha => 'a',
                UnityVersionType.Beta => 'b',
                UnityVersionType.China => 'c',
                UnityVersionType.Final => 'f',
                UnityVersionType.Patch => 'p',
                UnityVersionType.Experimental => 'x',
                _ => 'u',
            };
        }

        public static UnityVersionType ToUnityVersionType(this char c)
        {
            return c switch
            {
                'a' => UnityVersionType.Alpha,
                'b' => UnityVersionType.Beta,
                'c' => UnityVersionType.China,
                'f' => UnityVersionType.Final,
                'p' => UnityVersionType.Patch,
                'x' => UnityVersionType.Experimental,
                _ => throw new ArgumentException($"There is no version type {c}", "c"),
            };
        }
    }

}
