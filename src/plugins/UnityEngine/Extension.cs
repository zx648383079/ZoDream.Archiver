using System;

namespace UnityEngine
{
    public static class Extension
    {
        public static char ToCharacter(this VersionType type)
        {
            return type switch
            {
                VersionType.Alpha => 'a',
                VersionType.Beta => 'b',
                VersionType.China => 'c',
                VersionType.Final => 'f',
                VersionType.Patch => 'p',
                VersionType.TuanJie => 't',
                VersionType.Experimental => 'x',
                _ => 'u',
            };
        }

        public static VersionType ToVersionType(this char c)
        {
            return c switch
            {
                'a' => VersionType.Alpha,
                'b' => VersionType.Beta,
                'c' => VersionType.China,
                'f' => VersionType.Final,
                'p' => VersionType.Patch,
                't' => VersionType.TuanJie,
                'x' => VersionType.Experimental,
                _ => throw new ArgumentException($"There is no version type {c}", "c"),
            };
        }
    }
}
