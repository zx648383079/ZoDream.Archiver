using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZoDream.Shared.IO
{
    /// <summary>
    /// 在文件中查找字符
    /// </summary>
    public class StreamFinder
    {
        public StreamFinder(byte[][][] items)
        {
            _items = items;
        }

        public StreamFinder(params string[] items): this([items])
        {
            
        }

        public StreamFinder(string[][] items) : this(items, Encoding.ASCII)
        {

        }

        public StreamFinder(Dictionary<string, string> items)
            : this(items.Select(i => new string[] { i.Key, i.Value }).ToArray())
        {
        }

        public StreamFinder(string[][] items, Encoding encoding)
        {
            _items = items.Select(group => group.Select(item => encoding.GetBytes(item)).ToArray()).ToArray();
        }

        private readonly byte[][][] _items;
        private List<StreamMatchResult>[] _matchItems = [];
        private List<StreamMatchProcess>[] _processItems = [];
        /// <summary>
        /// 是否只需要找第一个
        /// </summary>
        public bool IsMatchFirst { get; set; } = true;

        private long _position = -1;

        public bool IsMatched => _matchItems.Where(item => item.Count > 0).Any();

        public IEnumerable<long> BeginPosition => _matchItems.SelectMany(item => item.Select(i => i.Begin));
        public IEnumerable<long> EndPosition => _matchItems.SelectMany(item => item.Select(i => i.End));

        public void Reset()
        {
            _matchItems = _items.Select(i => new List<StreamMatchResult>()).ToArray();
            _processItems = _items.Select(i => new List<StreamMatchProcess>()).ToArray();
            _position = -1;
        }

        public bool MatchFile(Stream reader)
        {
            Reset();
            _position = reader.Position - 1;
            while (true)
            {
                var code = reader.ReadByte();
                if (code < 0)
                {
                    break;
                }
                MatchByte(code);
                if (IsMatchFirst && IsMatched)
                {
                    break;
                }
            }
            return IsMatched;
        }

        public bool MatchByte(int code)
        {
            if (code < 0)
            {
                return false;
            }
            _position++;
            for (int i = 0; i < _items.Length; i++)
            {
                CheckProcess(i, _items[i], code);
                TryPushProcess(i, _items[i], code);
            }
            return IsMatched;
        }

        protected void CheckProcess(int i, byte[][] rule, int code)
        {
            if (_processItems[i].Count == 0)
            {
                return;
            }
            var items = _processItems[i];
            _processItems[i] = [];
            foreach (var item in items)
            {
                var next = item.Offset + 1;
                if (IsMatchCode(rule[item.Index][next], code))
                {
                    PushProcess(i, rule, item.Index, next, item.Begin);
                    continue;
                }
                if (item.Offset < 0)
                {
                    PushProcess(i, rule, item.Index, item.Offset, item.Begin);
                    continue;
                }
            }
        }

        protected void TryPushProcess(int i, byte[][] rule, int code)
        {
            if (!IsMatchCode(rule[0][0], code))
            {
                return;
            }
            PushProcess(i, rule, 0, 0, _position);
        }

        protected void PushProcess(int i, byte[][] rule, int ruleIndex, int codeIndex, long position)
        {
            if (codeIndex == rule[ruleIndex].Length - 1)
            {
                ruleIndex++;
                codeIndex = -1;
            }
            if (ruleIndex >= rule.Length)
            {
                _matchItems[i].Add(new(position, _position));
                return;
            }
            _processItems[i].Add(new(position, ruleIndex, codeIndex));
        }

        public static bool IsMatchCode(byte input, int code)
        {
            return input == code;
        }
    }

    internal class StreamMatchResult(long begin, long end)
    {
        public long Begin { get; set; } = begin;

        public long End { get; set; } = end;
    }

    internal class StreamMatchProcess(long begin, int index, int offset)
    {
        public long Begin { get; set; } = begin;

        public int Index { get; set; } = index;
        public int Offset { get; set; } = offset;
    }
}
