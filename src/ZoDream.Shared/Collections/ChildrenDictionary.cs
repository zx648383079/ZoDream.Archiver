using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ZoDream.Shared.Collections
{
    [DebuggerDisplay("{Count}")]
    public sealed class ChildrenDictionary<T, TParent>(TParent parent) : IReadOnlyDictionary<string, T>, IDictionary<string, T>
        where T : class, IChildOfDictionary<TParent>
        where TParent : class
    {
        #region data

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly TParent _parent = parent;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private Dictionary<string, T>? _items;

        #endregion

        #region Properties

        IEnumerable<string> IReadOnlyDictionary<string, T>.Keys => this.Keys;
        public ICollection<string> Keys => _items == null ? Array.Empty<string>() : (ICollection<string>)_items.Keys;

        IEnumerable<T> IReadOnlyDictionary<string, T>.Values => this.Values;
        public ICollection<T> Values => _items == null ? Array.Empty<T>() : (ICollection<T>)_items.Values;

        public int Count => _items == null ? 0 : _items.Count;

        public bool IsReadOnly => false;

        #endregion

        #region API

        public T this[string key] {
            get => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();
            set => Add(key, value);
        }

        public void Clear()
        {
            if (_items == null)
            {
                return;
            }

            foreach (var item in _items)
            {
                item.Value.SetLogicalParent(null, null);
            }

            _items = null;
        }

        public void Add(string key, T value)
        {
            _items ??= [];

            Remove(key);

            if (value == null)
            {
                return;
            }

            value.SetLogicalParent(_parent, key);

            _items[key] = value;
        }

        public bool Remove(string key)
        {
            if (_items == null)
            {
                return false;
            }

            if (!_items.TryGetValue(key, out var oldValue))
            {
                return false;
            }

            oldValue?.SetLogicalParent(null, null);

            var r = _items.Remove(key);

            if (_items.Count == 0)
            {
                _items = null;
            }

            return r;
        }

        public bool ContainsKey(string key)
        {
            if (_items == null)
            {
                return false;
            }
            return _items.ContainsKey(key);
        }

        public bool TryGetValue(string key, [NotNullWhen(true)] out T? value)
        {
            if (_items == null) 
            {
                value = default; 
                return false;
            }
            return _items.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            var c = _items ?? Enumerable.Empty<KeyValuePair<string, T>>();
            return c.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            var c = _items ?? Enumerable.Empty<KeyValuePair<string, T>>();
            return c.GetEnumerator();
        }

        public void Add(KeyValuePair<string, T> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<string, T> item)
        {
            return ContainsKey(item.Key);
        }

        public bool Remove(KeyValuePair<string, T> item)
        {
            return Remove(item.Key);
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            if (_items == null)
            {
                return;
            }
            foreach (var kvp in _items)
            {
                array[arrayIndex++] = kvp;
            }
        }

        #endregion
    }
}
