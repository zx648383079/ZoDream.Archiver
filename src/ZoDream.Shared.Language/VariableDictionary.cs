using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ZoDream.Shared.Language
{
    public class VariableDictionary : IDictionary<string, string>
    {

        private readonly List<string> _keys = [];
        private readonly List<string> _values = [];

        public string this[string key] { get => TryGetValue(key, out var val) ? val : string.Empty; set => Add(key, value); }

        public ICollection<string> Keys => _keys;

        public ICollection<string> Values => _values;

        public int Count => _keys.Count;

        public bool IsReadOnly => false;

        public int IndexOfKey(string key)
        {
            return _keys.IndexOf(key);
        }

        public void Add(string key, string value)
        {
            var i = IndexOfKey(key);
            if (i >= 0)
            {
                _values[i] = value;
                return;
            }
            _keys.Add(key);
            _values.Add(value);
        }

        public void Add(KeyValuePair<string, string> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _keys.Clear();
            _values.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            for (int i = _keys.Count - 1; i >= 0; i--)
            {
                if (_keys[i] == item.Key && _values[i] == item.Value)
                {
                    return true;
                }
            }
            return false;
        }

        public bool ContainsKey(string key)
        {
            return _keys.Contains(key); ;
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            for (int i = 0; i < array.Length - arrayIndex; i++)
            {
                array[arrayIndex + i] = new KeyValuePair<string, string>(_keys[i], _values[i]);
            }
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            for (int i = _keys.Count - 1; i >= 0; i--)
            {
                yield return new KeyValuePair<string, string>(_keys[i], _values[i]);
            }
        }

        public bool Remove(string key)
        {
            for (int i = _keys.Count - 1; i >= 0; i--)
            {
                if (_keys[i] == key)
                {
                    RemoveAt(i);
                }
            }
            return true;
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            for (int i = _keys.Count - 1; i >= 0; i--)
            {
                if (_keys[i] == item.Key && _values[i] == item.Value)
                {
                    RemoveAt(i);
                }
            }
            return true;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || _keys.Count <= index)
            {
                return;
            }
            _keys.RemoveAt(index);
            _values.RemoveAt(index);
        }

        public void Rename(string oldName, string newName)
        {
            if (oldName == newName)
            {
                return;
            }
            Remove(newName);
            for (var i = _keys.Count - 1; i >= 0; i--)
            {
                if (_keys[i] == oldName)
                {
                    _keys[i] = newName;
                }
            }
        }

        public void Remove(Func<string, bool> where)
        {
            for (var i = _keys.Count - 1; i >= 0; i--)
            {
                if (where.Invoke(_keys[i]))
                {
                    RemoveAt(i);
                }
            }
        }

        public void RemoveStartWith(params string[] prefix)
        {
            Remove(key => {
                foreach (var item in prefix)
                {
                    if (key.StartsWith(item))
                    {
                        return true;
                    }
                }
                return false;
            });
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
        {
            for (int i = _keys.Count - 1; i >= 0; i--)
            {
                if (_keys[i] == key)
                {
                    value = _values[i];
                    return true;
                }
            }
            value = null;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
