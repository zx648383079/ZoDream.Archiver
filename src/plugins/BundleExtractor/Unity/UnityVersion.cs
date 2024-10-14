using System;
using System.Numerics;
using System.Text.RegularExpressions;
using ZoDream.BundleExtractor.Unity;

namespace ZoDream.BundleExtractor.Models
{
    public readonly struct UnityVersion : IComparisonOperators<UnityVersion, UnityVersion, bool>, IEqualityOperators<UnityVersion, UnityVersion, bool>, IMinMaxValue<UnityVersion>, IEquatable<UnityVersion>, IComparable, IComparable<UnityVersion>
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
        public UnityVersionType Type => (UnityVersionType)((m_data >> 8) & 0xFF);

        //
        // 摘要:
        //     The last number in a Unity version string
        public byte TypeNumber => (byte)(m_data & 0xFF);

        //
        // 摘要:
        //     The minimum value this type can have
        public static UnityVersion MinVersion { get; } = new UnityVersion(0uL);


        //
        // 摘要:
        //     The maximum value this type can have
        public static UnityVersion MaxVersion { get; } = new UnityVersion(ulong.MaxValue);


        static UnityVersion IMinMaxValue<UnityVersion>.MaxValue => MaxVersion;

        static UnityVersion IMinMaxValue<UnityVersion>.MinValue => MinVersion;

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

        public bool Equals(ushort major, ushort minor, ushort build, UnityVersionType type)
        {
            return this == From(major, minor, build, type);
        }

        public bool Equals(ushort major, ushort minor, ushort build, UnityVersionType type, byte typeNumber)
        {
            return this == new UnityVersion(major, minor, build, type, typeNumber);
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

        public bool LessThan(ushort major, ushort minor, ushort build, UnityVersionType type)
        {
            return this < From(major, minor, build, type);
        }

        public bool LessThan(ushort major, ushort minor, ushort build, UnityVersionType type, byte typeNumber)
        {
            return this < new UnityVersion(major, minor, build, type, typeNumber);
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

        public bool LessThanOrEquals(ushort major, ushort minor, ushort build, UnityVersionType type)
        {
            return this <= From(major, minor, build, type);
        }

        public bool LessThanOrEquals(ushort major, ushort minor, ushort build, UnityVersionType type, byte typeNumber)
        {
            return this <= new UnityVersion(major, minor, build, type, typeNumber);
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

        public bool GreaterThan(ushort major, ushort minor, ushort build, UnityVersionType type)
        {
            return this > From(major, minor, build, type);
        }

        public bool GreaterThan(ushort major, ushort minor, ushort build, UnityVersionType type, byte typeNumber)
        {
            return this > new UnityVersion(major, minor, build, type, typeNumber);
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

        public bool GreaterThanOrEquals(ushort major, ushort minor, ushort build, UnityVersionType type)
        {
            return this >= From(major, minor, build, type);
        }

        public bool GreaterThanOrEquals(ushort major, ushort minor, ushort build, UnityVersionType type, byte typeNumber)
        {
            return this >= new UnityVersion(major, minor, build, type, typeNumber);
        }

        public bool GreaterThanOrEquals(string version)
        {
            return this >= Parse(version);
        }

        private UnityVersion From(ushort major)
        {
            return new UnityVersion(((ulong)major << 48) | (0xFFFFFFFFFFFFuL & m_data));
        }

        private UnityVersion From(ushort major, ushort minor)
        {
            return new UnityVersion(((ulong)major << 48) | ((ulong)minor << 32) | (0xFFFFFFFFu & m_data));
        }

        private UnityVersion From(ushort major, ushort minor, ushort build)
        {
            return new UnityVersion(((ulong)major << 48) | ((ulong)minor << 32) | ((ulong)build << 16) | (0xFFFF & m_data));
        }

        private UnityVersion From(ushort major, ushort minor, ushort build, UnityVersionType type)
        {
            return new UnityVersion(((ulong)major << 48) | ((ulong)minor << 32) | ((ulong)build << 16) | ((ulong)type << 8) | (0xFF & m_data));
        }

        //
        // 摘要:
        //     Construct a new Unity version
        public UnityVersion(ushort major)
        {
            m_data = (ulong)major << 48;
        }

        //
        // 摘要:
        //     Construct a new Unity version
        public UnityVersion(ushort major, ushort minor)
        {
            m_data = ((ulong)major << 48) | ((ulong)minor << 32);
        }

        //
        // 摘要:
        //     Construct a new Unity version
        public UnityVersion(ushort major, ushort minor, ushort build)
        {
            m_data = ((ulong)major << 48) | ((ulong)minor << 32) | ((ulong)build << 16);
        }

        //
        // 摘要:
        //     Construct a new Unity version
        public UnityVersion(ushort major, ushort minor, ushort build, UnityVersionType type)
        {
            m_data = ((ulong)major << 48) | ((ulong)minor << 32) | ((ulong)build << 16) | ((ulong)type << 8);
        }

        //
        // 摘要:
        //     Construct a new Unity version
        public UnityVersion(ushort major, ushort minor, ushort build, UnityVersionType type, byte typeNumber)
        {
            m_data = ((ulong)major << 48) | ((ulong)minor << 32) | ((ulong)build << 16) | ((ulong)type << 8) | typeNumber;
        }

        private UnityVersion(ulong data)
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
        public static UnityVersion FromBits(ulong bits)
        {
            return new UnityVersion(bits);
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
            if (!(obj is UnityVersion other))
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
        public int CompareTo(UnityVersion other)
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
            if (obj is UnityVersion unityVersion)
            {
                return this == unityVersion;
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
        public bool Equals(UnityVersion other)
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
        public static UnityVersion Max(UnityVersion left, UnityVersion right)
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
        public static UnityVersion Min(UnityVersion left, UnityVersion right)
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
        public static ulong Distance(UnityVersion left, UnityVersion right)
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
        public UnityVersion GetClosestVersion(UnityVersion[] versions)
        {
            if (versions == null)
            {
                throw new ArgumentNullException("versions");
            }

            if (versions.Length == 0)
            {
                throw new ArgumentException("Length cannot be zero", "versions");
            }

            UnityVersion unityVersion = versions[0];
            ulong num = Distance(this, unityVersion);
            for (int i = 1; i < versions.Length; i++)
            {
                ulong num2 = Distance(this, versions[i]);
                if (num2 < num)
                {
                    num = num2;
                    unityVersion = versions[i];
                }
            }

            return unityVersion;
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
        public UnityVersion ChangeMajor(ushort value)
        {
            return new UnityVersion(value, Minor, Build, Type, TypeNumber);
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
        public UnityVersion ChangeMinor(ushort value)
        {
            return new UnityVersion(Major, value, Build, Type, TypeNumber);
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
        public UnityVersion ChangeBuild(ushort value)
        {
            return new UnityVersion(Major, Minor, value, Type, TypeNumber);
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
        public UnityVersion ChangeType(UnityVersionType value)
        {
            return new UnityVersion(Major, Minor, Build, value, TypeNumber);
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
        public UnityVersion ChangeTypeNumber(byte value)
        {
            return new UnityVersion(Major, Minor, Build, Type, value);
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
        public static bool operator ==(UnityVersion left, UnityVersion right)
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
        public static bool operator !=(UnityVersion left, UnityVersion right)
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
        public static bool operator >(UnityVersion left, UnityVersion right)
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
        public static bool operator >=(UnityVersion left, UnityVersion right)
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
        public static bool operator <(UnityVersion left, UnityVersion right)
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
        public static bool operator <=(UnityVersion left, UnityVersion right)
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
            return ToString(UnityVersionFormatFlags.Default);
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
        public string ToString(UnityVersionFormatFlags flags)
        {
            if ((flags & UnityVersionFormatFlags.ExcludeType) == 0)
            {
                if (Type == UnityVersionType.China && (flags & UnityVersionFormatFlags.UseShortChineseFormat) == 0)
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
        public string ToString(UnityVersionFormatFlags flags, ReadOnlySpan<char> customEngineString = default(ReadOnlySpan<char>))
        {
            if (customEngineString.Length != 0)
            {
                if ((flags & UnityVersionFormatFlags.ExcludeType) == 0)
                {
                    if (Type == UnityVersionType.China && (flags & UnityVersionFormatFlags.UseShortChineseFormat) == 0)
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
        public static UnityVersion Parse(string s)
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
        public static UnityVersion Parse(string s, out string? customEngine)
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
        public static bool TryParse(string s, out UnityVersion version, out string? customEngine)
        {
            if (string.IsNullOrEmpty(s))
            {
                customEngine = null;
                version = default(UnityVersion);
                return false;
            }
            var match = UnityVersionRegexes.China().Match(s);
            if (match.Success)
            {
                int num = int.Parse(match.Groups[1].Value);
                int num2 = int.Parse(match.Groups[2].Value);
                int num3 = int.Parse(match.Groups[3].Value);
                int num4 = int.Parse(match.Groups[4].Value);
                customEngine = GetNullableString(match.Groups[5]);
                version = new UnityVersion((ushort)num, (ushort)num2, (ushort)num3, UnityVersionType.China, (byte)num4);
                return true;
            }
            match = UnityVersionRegexes.Normal().Match(s);
            if (match.Success)
            {
                int num5 = int.Parse(match.Groups[1].Value);
                int num6 = int.Parse(match.Groups[2].Value);
                int num7 = int.Parse(match.Groups[3].Value);
                char c = match.Groups[4].Value[0];
                int num8 = int.Parse(match.Groups[5].Value);
                customEngine = GetNullableString(match.Groups[6]);
                version = new UnityVersion((ushort)num5, (ushort)num6, (ushort)num7, c.ToUnityVersionType(), (byte)num8);
                return true;
            }
            match = UnityVersionRegexes.MajorMinorBuild().Match(s);
            if (match.Success)
            {
                int num9 = int.Parse(match.Groups[1].Value);
                int num10 = int.Parse(match.Groups[2].Value);
                int num11 = int.Parse(match.Groups[3].Value);
                customEngine = null;
                version = ((num9 != 0 || num10 != 0 || num11 != 0) ? new UnityVersion((ushort)num9, (ushort)num10, (ushort)num11, UnityVersionType.Final, 1) : default(UnityVersion));
                return true;
            }
            match = UnityVersionRegexes.MajorMinor().Match(s);
            if (match.Success)
            {
                int num12 = int.Parse(match.Groups[1].Value);
                int num13 = int.Parse(match.Groups[2].Value);
                customEngine = null;
                version = ((num12 != 0 || num13 != 0) ? new UnityVersion((ushort)num12, (ushort)num13, 0, UnityVersionType.Final, 1) : default(UnityVersion));
                return true;
            }
            match = UnityVersionRegexes.Major().Match(s);
            if (match.Success)
            {
                int num14 = int.Parse(match.Groups[1].Value);
                customEngine = null;
                version = ((num14 != 0) ? new UnityVersion((ushort)num14, 0, 0, UnityVersionType.Final, 1) : default(UnityVersion));
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

    internal partial class UnityVersionRegexes
    {
        [GeneratedRegex("([0-9]+)")]
        public static partial Regex Major();


        [GeneratedRegex("([0-9]+)\\.([0-9]+)")]
        public static partial Regex MajorMinor();

        [GeneratedRegex("([0-9]+)\\.([0-9]+)\\.([0-9]+)")]
        public static partial Regex MajorMinorBuild();

        [GeneratedRegex("([0-9]+)\\.([0-9]+)\\.([0-9]+)\\.?([abcfpx])([0-9]+)((?:.|[\\r\\n])+)?")]
        public static partial Regex Normal();

        [GeneratedRegex("([0-9]+)\\.([0-9]+)\\.([0-9]+)\\.?f1c([0-9]+)((?:.|[\\r\\n])+)?")]
        public static partial Regex China();
    }
}
