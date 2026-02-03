using System.Collections.Concurrent;
using ZoDream.Shared.Bundle;
using System.Diagnostics.CodeAnalysis;
using System;
using System.Globalization;
using System.Text;

namespace ZoDream.BundleExtractor
{
    public class PackageArguments : ConcurrentDictionary<string, ICommandArgument>, ICommandArguments
    {
        public bool Contains(string cmd)
        {
            return ContainsKey(cmd);
        }
        public void Add(string cmd, ICommandArgument args)
        {
            TryAdd(cmd, args);
        }

        public bool TryGet<T>(string cmd, [NotNullWhen(true)] out T? args) where T : ICommandArgument
        {
            if (TryGetValue(cmd, out var res))
            {
                args = (T)res;
                return true;
            }
            args = default;
            return false;
        }

        public void Parse(string package)
        {
            foreach (var item in package.Split('.'))
            {
                var args = item.Split(':');
                var cmd = args[0].Trim();
                if (string.IsNullOrEmpty(cmd))
                {
                    continue;
                }
                if (args.Length == 1)
                {
                    Add(cmd, EmptyCommandArgument.Instance);
                    continue;
                }
                args = args[1].Split(',');
                switch (cmd)
                {
                    case XorCommandArgument.TagName:
                        Add(cmd, ParseXor(args));
                        break;
                    case KeyCommandArgument.TagName:
                        Add(cmd, new KeyCommandArgument(Convert.FromHexString(args[0].Trim())));
                        break;
                    case SkipCommandArgument.TagName:
                        Add(cmd, new SkipCommandArgument(ParseInt64(args[0])));
                        break;
                    default:
                        Add(cmd, EmptyCommandArgument.Instance);
                        break;
                }
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var item in this)
            {
                if (sb.Length > 0)
                {
                    sb.Append('.');
                }
                if (item.Value is EmptyCommandArgument)
                {
                    sb.Append(item.Key);
                    continue;
                }
                sb.Append(item.Value.ToString());
            }
            return sb.ToString();
        }

        public static ICommandArguments Create(string? package)
        {
            var res = new PackageArguments();
            if (!string.IsNullOrWhiteSpace(package))
            {
                res.Parse(package);
            }
            return res;
        }

        public static ICommandArgument ParseXor(string[] items)
        {
            if (items.Length < 2)
            {
                return new XorCommandArgument(Convert.FromHexString(items[0].Trim()));
            }
            return new XorCommandArgument(Convert.FromHexString(items[0].Trim()),
                ParseInt64(items[1]));
        }

        public static long ParseInt64(string val)
        {
            return long.TryParse(val.Trim(), NumberStyles.HexNumber, null, out var res) ? res : 0;
        }
    }
}
