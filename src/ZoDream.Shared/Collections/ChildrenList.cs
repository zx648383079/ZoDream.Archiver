using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ZoDream.Shared.Collections
{
    [DebuggerDisplay("{Count}")]
    public sealed class ChildrenList<T, TParent>(TParent parent) : IList<T>, IReadOnlyList<T>
        where T : class, IChildOfList<TParent>
        where TParent : class
    {

        #region data

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly TParent _parent = parent;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private List<T>? _items;

        #endregion

        #region properties

        public T this[int index] {
            get {
                if (_items == null) throw new ArgumentOutOfRangeException(nameof(index));
                return _items[index];
            }

            set {
                if (_items == null) throw new ArgumentOutOfRangeException(nameof(index));

                if (_items[index] == value)
                {
                    return;
                }

                _items[index]?.SetLogicalParent(null, -1);

                _items[index] = value;

                _items[index]?.SetLogicalParent(_parent, index);
            }
        }

        public int Count => _items == null ? 0 : _items.Count;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsReadOnly => false;

        #endregion

        #region API

        public bool Contains(T item)
        {
            return _items?.Contains(item) ?? false;
        }

        public int IndexOf(T item)
        {
            return _items?.IndexOf(item) ?? -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (_items == null) return;
            _items.CopyTo(array, arrayIndex);
        }

        public void Add(T item)
        {
            _items ??= [];

            var idx = _items.Count;
            _items.Add(item);
            item.SetLogicalParent(_parent, idx);
        }

        public void Clear()
        {
            if (_items == null) return;

            foreach (var item in _items)
            {
                item.SetLogicalParent(null, -1);
            }

            _items = null;
        }

        public void Insert(int index, T item)
        {
            _items ??= [];
            _items.Insert(index, item);
            for (var i = index; i < _items.Count; ++i)
            {
                item = _items[i];
                if (item == null)
                {
                    continue;
                }
                item.SetLogicalParent(_parent, i);
            }
        }

        public bool Remove(T item)
        {
            var idx = IndexOf(item);
            if (idx < 0) return false;
            RemoveAt(idx);

            return true;
        }

        public void RemoveAt(int index)
        {
            if (_items == null) throw new ArgumentOutOfRangeException(nameof(index));
            if (index < 0 || index >= _items.Count) throw new ArgumentOutOfRangeException(nameof(index));

            var item = _items[index];
            _items.RemoveAt(index);

            item?.SetLogicalParent(null, -1);

            for (int i = index; i < _items.Count; ++i)
            {
                item = _items[i];

                item.SetLogicalParent(_parent, i);
            }

            if (_items.Count == 0) _items = null;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items?.GetEnumerator() ?? Enumerable.Empty<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items?.GetEnumerator() ?? Enumerable.Empty<T>().GetEnumerator();
        }

        #endregion
    }
}
