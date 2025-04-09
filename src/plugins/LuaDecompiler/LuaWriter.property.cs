using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ZoDream.LuaDecompiler
{
    public partial class LuaWriter
    {
        /// <summary>
        /// 全局注册的
        /// </summary>
        private readonly List<string> _envKeys = [];
        private readonly List<string> _envValues = [];

        private int IndexOf(string key)
        {
            return _envKeys.IndexOf(key);
        }

        private bool Contains(string key)
        {
            return _envKeys.Contains(key);
        }

        private void Add(string key, string value)
        {
            var i = IndexOf(key);
            if (i >= 0)
            {
                _envValues[i] = value;
                return;
            }
            _envKeys.Add(key);
            _envValues.Add(value);
        }

        private void Remove(string key)
        {
            for (int i = _envKeys.Count - 1; i >= 0; i--)
            {
                if (_envKeys[i] == key)
                {
                    RemoveAt(i);
                }
            }
        }

        private void RemoveAt(int index)
        {
            if (index < 0 || _envKeys.Count <= index)
            {
                return;
            }
            _envKeys.RemoveAt(index);
            _envValues.RemoveAt(index);
        }

        private bool TryGet(string key, [NotNullWhen(true)] out string? val)
        {
            for (int i = _envKeys.Count - 1; i >= 0; i--)
            {
                if (_envKeys[i] == key)
                {
                    val = _envValues[i];
                    return true;
                }
            }
            val = null;
            return false;
        }

        private void Rename(string oldName, string newName)
        {
            if (oldName == newName)
            {
                return;
            }
            Remove(newName);
            for (var i = _envKeys.Count - 1; i >= 0; i--)
            {
                if (_envKeys[i] == oldName)
                {
                    _envKeys[i] = newName;
                }
            }
        }
        /// <summary>
        /// 移除临时变量
        /// </summary>
        private void RemoveTemporary()
        {
            for (var i = _envKeys.Count - 1; i >= 0; i--)
            {
                if (_envKeys[i].StartsWith("slot"))
                {
                    RemoveAt(i);
                }
            }
        }
    }
}
