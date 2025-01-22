using System;

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
        // t 团结引擎
        TuanJie = 6,
        //
        // 摘要:
        //     The minimum valid value for this enumeration
        MinValue = 0,
        //
        // 摘要:
        //     The maximum valid value for this enumeration
        MaxValue = 6
    }
    public static class UnityVersionTypeExtensions
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
                UnityVersionType.TuanJie => 't',
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
                't' => UnityVersionType.TuanJie,
                'x' => UnityVersionType.Experimental,
                _ => throw new ArgumentException($"There is no version type {c}", "c"),
            };
        }
    }

}
