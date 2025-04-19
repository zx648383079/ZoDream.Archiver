using System;
using System.Numerics;
using System.Text.RegularExpressions;

namespace UnityEngine
{
    public struct Version : IComparisonOperators<Version, Version, bool>, IEqualityOperators<Version, Version, bool>, IMinMaxValue<Version>, IEquatable<Version>, IComparable, IComparable<Version>
    {
        private const ulong subMajorMask = 281474976710655uL;

        private const ulong subMinorMask = 4294967295uL;

        private const ulong subBuildMask = 65535uL;

        private const ulong subTypeMask = 255uL;

        private const int majorOffset = 48;

        private const int minorOffset = 32;

        private const int buildOffset = 16;

        private const int typeOffset = 8;

        private const ulong byteMask = 255uL;

        private const ulong ushortMask = 65535uL;

        private readonly ulong m_data;

        //
        // 摘要:
        //     The first number in a Unity version string
        public ushort Major => (ushort)((m_data >> 48) & 0xFFFF);

        //
        // 摘要:
        //     The second number in a Unity version string
        public ushort Minor => (ushort)((m_data >> 32) & 0xFFFF);

        //
        // 摘要:
        //     The third number in a Unity version string
        public ushort Build => (ushort)((m_data >> 16) & 0xFFFF);

        //
        // 摘要:
        //     The letter in a Unity version string
        public VersionType Type => (VersionType)((m_data >> 8) & 0xFF);

        //
        // 摘要:
        //     The last number in a Unity version string
        public byte TypeNumber => (byte)(m_data & 0xFF);

        //
        // 摘要:
        //     The minimum value this type can have
        public static Version MinVersion { get; } = new Version(0uL);


        //
        // 摘要:
        //     The maximum value this type can have
        public static Version MaxVersion { get; } = new Version(ulong.MaxValue);


        static Version IMinMaxValue<Version>.MaxValue => MaxVersion;

        static Version IMinMaxValue<Version>.MinValue => MinVersion;

        public bool Equals(ushort major)
        {
            return this == From(major);
        }

        public bool Equals(ushort major, ushort minor)
        {
            return this == From(major, minor);
        }

        public bool Equals(ushort major, ushort minor, ushort build)
        {
            return this == From(major, minor, build);
        }

        public bool Equals(ushort major, ushort minor, ushort build, VersionType type)
        {
            return this == From(major, minor, build, type);
        }

        public bool Equals(ushort major, ushort minor, ushort build, VersionType type, byte typeNumber)
        {
            return this == new Version(major, minor, build, type, typeNumber);
        }

        public bool Equals(string version)
        {
            return this == Parse(version);
        }

        public bool LessThan(ushort major)
        {
            return this < From(major);
        }

        public bool LessThan(ushort major, ushort minor)
        {
            return this < From(major, minor);
        }

        public bool LessThan(ushort major, ushort minor, ushort build)
        {
            return this < From(major, minor, build);
        }

        public bool LessThan(ushort major, ushort minor, ushort build, VersionType type)
        {
            return this < From(major, minor, build, type);
        }

        public bool LessThan(ushort major, ushort minor, ushort build, VersionType type, byte typeNumber)
        {
            return this < new Version(major, minor, build, type, typeNumber);
        }

        public bool LessThan(string version)
        {
            return this < Parse(version);
        }

        public bool LessThanOrEquals(ushort major)
        {
            return this <= From(major);
        }

        public bool LessThanOrEquals(ushort major, ushort minor)
        {
            return this <= From(major, minor);
        }

        public bool LessThanOrEquals(ushort major, ushort minor, ushort build)
        {
            return this <= From(major, minor, build);
        }

        public bool LessThanOrEquals(ushort major, ushort minor, ushort build, VersionType type)
        {
            return this <= From(major, minor, build, type);
        }

        public bool LessThanOrEquals(ushort major, ushort minor, ushort build, VersionType type, byte typeNumber)
        {
            return this <= new Version(major, minor, build, type, typeNumber);
        }

        public bool LessThanOrEquals(string version)
        {
            return this <= Parse(version);
        }

        public bool GreaterThan(ushort major)
        {
            return this > From(major);
        }

        public bool GreaterThan(ushort major, ushort minor)
        {
            return this > From(major, minor);
        }

        public bool GreaterThan(ushort major, ushort minor, ushort build)
        {
            return this > From(major, minor, build);
        }

        public bool GreaterThan(ushort major, ushort minor, ushort build, VersionType type)
        {
            return this > From(major, minor, build, type);
        }

        public bool GreaterThan(ushort major, ushort minor, ushort build, VersionType type, byte typeNumber)
        {
            return this > new Version(major, minor, build, type, typeNumber);
        }

        public bool GreaterThan(string version)
        {
            return this > Parse(version);
        }

        public bool GreaterThanOrEquals(ushort major)
        {
            return this >= From(major);
        }

        public bool GreaterThanOrEquals(ushort major, ushort minor)
        {
            return this >= From(major, minor);
        }

        public bool GreaterThanOrEquals(ushort major, ushort minor, ushort build)
        {
            return this >= From(major, minor, build);
        }

        public bool GreaterThanOrEquals(ushort major, ushort minor, ushort build, VersionType type)
        {
            return this >= From(major, minor, build, type);
        }

        public bool GreaterThanOrEquals(ushort major, ushort minor, ushort build, VersionType type, byte typeNumber)
        {
            return this >= new Version(major, minor, build, type, typeNumber);
        }

        public bool GreaterThanOrEquals(string version)
        {
            return this >= Parse(version);
        }

        private Version From(ushort major)
        {
            return new Version(((ulong)major << 48) | (0xFFFFFFFFFFFFuL & m_data));
        }

        private Version From(ushort major, ushort minor)
        {
            return new Version(((ulong)major << 48) | ((ulong)minor << 32) | (0xFFFFFFFFu & m_data));
        }

        private Version From(ushort major, ushort minor, ushort build)
        {
            return new Version(((ulong)major << 48) | ((ulong)minor << 32) | ((ulong)build << 16) | (0xFFFF & m_data));
        }

        private Version From(ushort major, ushort minor, ushort build, VersionType type)
        {
            return new Version(((ulong)major << 48) | ((ulong)minor << 32) | ((ulong)build << 16) | ((ulong)type << 8) | (0xFF & m_data));
        }

        //
        // 摘要:
        //     Construct a new Unity version
        public Version(ushort major)
        {
            m_data = (ulong)major << 48;
        }

        //
        // 摘要:
        //     Construct a new Unity version
        public Version(ushort major, ushort minor)
        {
            m_data = ((ulong)major << 48) | ((ulong)minor << 32);
        }

        //
        // 摘要:
        //     Construct a new Unity version
        public Version(ushort major, ushort minor, ushort build)
        {
            m_data = ((ulong)major << 48) | ((ulong)minor << 32) | ((ulong)build << 16);
        }

        //
        // 摘要:
        //     Construct a new Unity version
        public Version(ushort major, ushort minor, ushort build, VersionType type)
        {
            m_data = ((ulong)major << 48) | ((ulong)minor << 32) | ((ulong)build << 16) | ((ulong)type << 8);
        }

        //
        // 摘要:
        //     Construct a new Unity version
        public Version(ushort major, ushort minor, ushort build, VersionType type, byte typeNumber)
        {
            m_data = ((ulong)major << 48) | ((ulong)minor << 32) | ((ulong)build << 16) | ((ulong)type << 8) | typeNumber;
        }

        private Version(ulong data)
        {
            m_data = data;
        }

        //
        // 摘要:
        //     Converts this to its binary representation
        //
        // 返回结果:
        //     An unsigned long integer having the same bits as this
        public ulong GetBits()
        {
            return m_data;
        }

        //
        // 摘要:
        //     Converts a binary representation into its respective version
        //
        // 参数:
        //   bits:
        //     An unsigned long integer having the relevant bits
        //
        // 返回结果:
        //     A new Unity version with the cooresponding bits
        public static Version FromBits(ulong bits)
        {
            return new Version(bits);
        }

        //
        // 摘要:
        //     Compare to another object
        //
        // 参数:
        //   obj:
        //     Another object
        //
        // 返回结果:
        //     1 if this is larger or the other is not a Unity version
        //     -1 if this is smaller
        //     0 if equal
        public int CompareTo(object? obj)
        {
            if (!(obj is Version other))
            {
                return 1;
            }

            return CompareTo(other);
        }

        //
        // 摘要:
        //     Compare two Unity versions
        //
        // 参数:
        //   other:
        //     Another Unity version
        //
        // 返回结果:
        //     1 if this is larger
        //     -1 if this is smaller
        //     0 if equal
        public int CompareTo(Version other)
        {
            return m_data.CompareTo(other.m_data);
        }

        //
        // 摘要:
        //     Check equality with another object
        //
        // 参数:
        //   obj:
        //     Another object
        //
        // 返回结果:
        //     True if they're equal; false otherwise
        public override bool Equals(object? obj)
        {
            if (obj is Version o)
            {
                return this == o;
            }

            return false;
        }

        //
        // 摘要:
        //     Check equality with another Unity version
        //
        // 参数:
        //   other:
        //     Another Unity version
        //
        // 返回结果:
        //     True if they're equal; false otherwise
        public bool Equals(Version other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return m_data.GetHashCode();
        }

        //
        // 摘要:
        //     A maximizing function for Unity versions
        //
        // 参数:
        //   left:
        //     A Unity version
        //
        //   right:
        //     A Unity version
        //
        // 返回结果:
        //     The larger Unity version
        public static Version Max(Version left, Version right)
        {
            if (!(left > right))
            {
                return right;
            }

            return left;
        }

        //
        // 摘要:
        //     A minimizing function for Unity versions
        //
        // 参数:
        //   left:
        //     A Unity version
        //
        //   right:
        //     A Unity version
        //
        // 返回结果:
        //     The smaller Unity version
        public static Version Min(Version left, Version right)
        {
            if (!(left < right))
            {
                return right;
            }

            return left;
        }

        //
        // 摘要:
        //     A distance function for measuring version proximity
        //
        // 参数:
        //   left:
        //     A Unity version
        //
        //   right:
        //     A Unity version
        //
        // 返回结果:
        //     An ordinal number representing the distance between 2 versions. A value of zero
        //     means they're equal.
        //
        // 言论：
        //     The returned value is ordinal and should not be saved anywhere. It's only for
        //     runtime comparisons, such as finding the closest version in a list.
        public static ulong Distance(Version left, Version right)
        {
            if (left.m_data >= right.m_data)
            {
                return left.m_data - right.m_data;
            }

            return right.m_data - left.m_data;
        }

        //
        // 摘要:
        //     Get the closest Unity version in an array of versions using AssetRipper.Primitives.UnityVersion.Distance(AssetRipper.Primitives.UnityVersion,AssetRipper.Primitives.UnityVersion)
        //
        //
        // 参数:
        //   versions:
        //     The Unity version array
        //
        // 返回结果:
        //     The closest Unity version
        //
        // 异常:
        //   T:System.ArgumentNullException:
        //     The array is null
        //
        //   T:System.ArgumentException:
        //     The array is empty
        public Version GetClosestVersion(Version[] versions)
        {
            if (versions == null)
            {
                throw new ArgumentNullException("versions");
            }

            if (versions.Length == 0)
            {
                throw new ArgumentException("Length cannot be zero", "versions");
            }

            Version version = versions[0];
            ulong num = Distance(this, version);
            for (int i = 1; i < versions.Length; i++)
            {
                ulong num2 = Distance(this, versions[i]);
                if (num2 < num)
                {
                    num = num2;
                    version = versions[i];
                }
            }

            return version;
        }

        //
        // 摘要:
        //     Change AssetRipper.Primitives.UnityVersion.Major.
        //
        // 参数:
        //   value:
        //     The new value
        //
        // 返回结果:
        //     A new AssetRipper.Primitives.UnityVersion with the changed value.
        public Version ChangeMajor(ushort value)
        {
            return new Version(value, Minor, Build, Type, TypeNumber);
        }

        //
        // 摘要:
        //     Change AssetRipper.Primitives.UnityVersion.Minor.
        //
        // 参数:
        //   value:
        //     The new value
        //
        // 返回结果:
        //     A new AssetRipper.Primitives.UnityVersion with the changed value.
        public Version ChangeMinor(ushort value)
        {
            return new Version(Major, value, Build, Type, TypeNumber);
        }

        //
        // 摘要:
        //     Change AssetRipper.Primitives.UnityVersion.Build.
        //
        // 参数:
        //   value:
        //     The new value
        //
        // 返回结果:
        //     A new AssetRipper.Primitives.UnityVersion with the changed value.
        public Version ChangeBuild(ushort value)
        {
            return new Version(Major, Minor, value, Type, TypeNumber);
        }

        //
        // 摘要:
        //     Change AssetRipper.Primitives.UnityVersion.Type.
        //
        // 参数:
        //   value:
        //     The new value
        //
        // 返回结果:
        //     A new AssetRipper.Primitives.UnityVersion with the changed value.
        public Version ChangeType(VersionType value)
        {
            return new Version(Major, Minor, Build, value, TypeNumber);
        }

        //
        // 摘要:
        //     Change AssetRipper.Primitives.UnityVersion.TypeNumber.
        //
        // 参数:
        //   value:
        //     The new value
        //
        // 返回结果:
        //     A new AssetRipper.Primitives.UnityVersion with the changed value.
        public Version ChangeTypeNumber(byte value)
        {
            return new Version(Major, Minor, Build, Type, value);
        }

        //
        // 摘要:
        //     Equality operator
        //
        // 参数:
        //   left:
        //     The left Unity version
        //
        //   right:
        //     The right Unity version
        public static bool operator ==(Version left, Version right)
        {
            return left.m_data == right.m_data;
        }

        //
        // 摘要:
        //     Inequality operator
        //
        // 参数:
        //   left:
        //     The left Unity version
        //
        //   right:
        //     The right Unity version
        public static bool operator !=(Version left, Version right)
        {
            return left.m_data != right.m_data;
        }

        //
        // 摘要:
        //     Greater than
        //
        // 参数:
        //   left:
        //     The left Unity version
        //
        //   right:
        //     The right Unity version
        public static bool operator >(Version left, Version right)
        {
            return left.m_data > right.m_data;
        }

        //
        // 摘要:
        //     Greater than or equal to
        //
        // 参数:
        //   left:
        //     The left Unity version
        //
        //   right:
        //     The right Unity version
        public static bool operator >=(Version left, Version right)
        {
            return left.m_data >= right.m_data;
        }

        //
        // 摘要:
        //     Less than
        //
        // 参数:
        //   left:
        //     The left Unity version
        //
        //   right:
        //     The right Unity version
        public static bool operator <(Version left, Version right)
        {
            return left.m_data < right.m_data;
        }

        //
        // 摘要:
        //     Less than or equal to
        //
        // 参数:
        //   left:
        //     The left Unity version
        //
        //   right:
        //     The right Unity version
        public static bool operator <=(Version left, Version right)
        {
            return left.m_data <= right.m_data;
        }

        //
        // 摘要:
        //     Serialize the version as a string using AssetRipper.Primitives.UnityVersionFormatFlags.Default.
        //
        //
        // 返回结果:
        //     A new string like 2019.4.3f1
        public override string ToString()
        {
            return ToString(VersionFormatFlags.Default);
        }

        //
        // 摘要:
        //     Serialize the version as a string
        //
        // 参数:
        //   flags:
        //     The flags to control how the version is formatted
        //
        // 返回结果:
        //     A new string containing the formatted version.
        public string ToString(VersionFormatFlags flags)
        {
            if ((flags & VersionFormatFlags.ExcludeType) == 0)
            {
                if (Type == VersionType.China && (flags & VersionFormatFlags.UseShortChineseFormat) == 0)
                {
                    return $"{Major}.{Minor}.{Build}f1c{TypeNumber}";
                }

                return $"{Major}.{Minor}.{Build}{Type.ToCharacter()}{TypeNumber}";
            }

            return ToStringWithoutType();
        }

        //
        // 摘要:
        //     Serialize the version as a string
        //
        // 参数:
        //   flags:
        //     The flags to control how the version is formatted
        //
        //   customEngineString:
        //     The custom engine string to be appended
        //
        // 返回结果:
        //     A new string containing the formatted version.
        public string ToString(VersionFormatFlags flags, ReadOnlySpan<char> customEngineString = default(ReadOnlySpan<char>))
        {
            if (customEngineString.Length != 0)
            {
                if ((flags & VersionFormatFlags.ExcludeType) == 0)
                {
                    if (Type == VersionType.China && (flags & VersionFormatFlags.UseShortChineseFormat) == 0)
                    {
                        return $"{Major}.{Minor}.{Build}f1c{TypeNumber}{customEngineString}";
                    }

                    return $"{Major}.{Minor}.{Build}{Type.ToCharacter()}{TypeNumber}{customEngineString}";
                }

                return ToStringWithoutType();
            }

            return ToString(flags);
        }

        //
        // 摘要:
        //     Serialize the version as a string using only AssetRipper.Primitives.UnityVersion.Major,
        //     AssetRipper.Primitives.UnityVersion.Minor, and AssetRipper.Primitives.UnityVersion.Build.
        //
        //
        // 返回结果:
        //     A new string like 2019.4.3
        public string ToStringWithoutType()
        {
            return $"{Major}.{Minor}.{Build}";
        }

        //
        // 摘要:
        //     Parse a normal Unity version string
        //
        // 参数:
        //   s:
        //     A string to parse
        //
        // 返回结果:
        //     The parsed Unity version
        //
        // 异常:
        //   T:System.ArgumentException:
        //     If the string is in an invalid format
        public static Version Parse(string s)
        {
            return Parse(s, out _);
        }

        //
        // 摘要:
        //     Parse a normal Unity version string
        //
        // 参数:
        //   s:
        //     A string to parse
        //
        //   customEngine:
        //     Not null if this version was generated by a custom Unity Engine.
        //
        // 返回结果:
        //     The parsed Unity version
        //
        // 异常:
        //   T:System.ArgumentException:
        //     If the string is in an invalid format
        public static Version Parse(string s, out string? customEngine)
        {
            if (!TryParse(s, out var version, out customEngine))
            {
                throw new ArgumentException("Invalid version format: " + s, "s");
            }

            return version;
        }

        //
        // 摘要:
        //     Try to parse a normal Unity version string
        //
        // 参数:
        //   s:
        //     A string to parse
        //
        //   version:
        //     The parsed Unity version
        //
        //   customEngine:
        //     Not null if this version was generated by a custom Unity Engine.
        //
        // 返回结果:
        //     True if parsing was successful
        public static bool TryParse(string s, out Version version, out string? customEngine)
        {
            if (string.IsNullOrEmpty(s))
            {
                customEngine = null;
                version = default;
                return false;
            }
            var match = VersionRegexes.China().Match(s);
            if (match.Success)
            {
                int num = int.Parse(match.Groups[1].Value);
                int num2 = int.Parse(match.Groups[2].Value);
                int num3 = int.Parse(match.Groups[3].Value);
                int num4 = int.Parse(match.Groups[5].Value);
                customEngine = GetNullableString(match.Groups[6]);
                version = new Version((ushort)num, (ushort)num2, (ushort)num3,
                    match.Groups[4].Value.Contains('t') ?
                    VersionType.TuanJie : VersionType.China,
                    (byte)num4);
                return true;
            }
            match = VersionRegexes.Normal().Match(s);
            if (match.Success)
            {
                int num5 = int.Parse(match.Groups[1].Value);
                int num6 = int.Parse(match.Groups[2].Value);
                int num7 = int.Parse(match.Groups[3].Value);
                char c = match.Groups[4].Value[0];
                int num8 = int.Parse(match.Groups[5].Value);
                customEngine = GetNullableString(match.Groups[6]);
                version = new Version((ushort)num5, (ushort)num6, (ushort)num7, c.ToVersionType(), (byte)num8);
                return true;
            }
            match = VersionRegexes.MajorMinorBuild().Match(s);
            if (match.Success)
            {
                int num9 = int.Parse(match.Groups[1].Value);
                int num10 = int.Parse(match.Groups[2].Value);
                int num11 = int.Parse(match.Groups[3].Value);
                customEngine = null;
                version = ((num9 != 0 || num10 != 0 || num11 != 0) ? new Version((ushort)num9, (ushort)num10, (ushort)num11, VersionType.Final, 1) : default(Version));
                return true;
            }
            match = VersionRegexes.MajorMinor().Match(s);
            if (match.Success)
            {
                int num12 = int.Parse(match.Groups[1].Value);
                int num13 = int.Parse(match.Groups[2].Value);
                customEngine = null;
                version = ((num12 != 0 || num13 != 0) ? new Version((ushort)num12, (ushort)num13, 0, VersionType.Final, 1) : default(Version));
                return true;
            }
            match = VersionRegexes.Major().Match(s);
            if (match.Success)
            {
                int num14 = int.Parse(match.Groups[1].Value);
                customEngine = null;
                version = ((num14 != 0) ? new Version((ushort)num14, 0, 0, VersionType.Final, 1) : default(Version));
                return true;
            }

            customEngine = null;
            version = default;
            return false;
            static string? GetNullableString(Capture capture)
            {
                if (capture.Length != 0)
                {
                    return capture.Value;
                }

                return null;
            }
        }
    }

    internal partial class VersionRegexes
    {
        [GeneratedRegex("([0-9]+)")]
        public static partial Regex Major();


        [GeneratedRegex("([0-9]+)\\.([0-9]+)")]
        public static partial Regex MajorMinor();

        [GeneratedRegex("([0-9]+)\\.([0-9]+)\\.([0-9]+)")]
        public static partial Regex MajorMinorBuild();

        [GeneratedRegex("([0-9]+)\\.([0-9]+)\\.([0-9]+)\\.?([abcfpx])([0-9]+)((?:.|[\\r\\n])+)?")]
        public static partial Regex Normal();

        [GeneratedRegex("([0-9]+)\\.([0-9]+)\\.([0-9]+)(\\.?f1c|t)([0-9]+)((?:.|[\\r\\n])+)?")]
        public static partial Regex China();
    }
}
