using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Alloy.UiLib.Utils;

/// <summary>
/// List that lets you add items during enumeration, but will not be enumerated.
/// Can also remove items during loop but only get removed after enumeration.
/// Only 1 enumerator allowed
/// </summary>
[CollectionBuilder(typeof(CachedListFactory), nameof(CachedListFactory.Create))]
internal class CachedList<T> {

    private class Entry(T item) {
        public readonly T Item = item;
        public bool ToRemove;

        public static implicit operator Entry(T item) => new (item);
    }

    private readonly List<Entry> _items = [];

    private bool _isEnumerating;

    public bool Contains(T item) => _items.Count != 0 && IndexOf(item) >= 0;

    public int IndexOf(T item) {
        var index = 0;
        foreach (var entry in _items) {
            if (!entry.ToRemove && EqualityComparer<T>.Default.Equals(entry.Item, item)) {
                return index;
            }

            index++;
        }

        return -1;
    }

    public void Add(T item) => _items.Add(item);

    public bool Remove(T item) {
        var index = IndexOf(item);
        if (index < 0) {
            return false;
        }

        RemoveAt(index);
        return true;
    }

    public void RemoveAt(int index) {
        if (_isEnumerating) {
            _items[index].ToRemove = true;
            return;
        }

        _items.RemoveAt(index);
    }

    public void Clear() {
        if (_isEnumerating) {
            foreach (var entry in _items) {
                entry.ToRemove = true;
            }
            return;
        }

        _items.Clear();
    }

    public CachedEnumerator GetEnumerator() {
        if (_isEnumerating) {
            throw new InvalidOperationException("Only one enumerator is allowed at a time.");
        }

        _isEnumerating = true;
        return new CachedEnumerator(this);
    }

    public struct CachedEnumerator(CachedList<T> list) : IDisposable {
        private readonly int _cachedCount = list._items.Count;
        private int _index = -1;
        
        public bool MoveNext() {
            _index++;
            
            if (_index >= _cachedCount) {
                return false;
            }

            Current = list._items[_index].Item;
            return true;
        }
        
        public T Current { get; private set; } = default;
        
        public void Dispose() {
            list._items.RemoveAll(e => e.ToRemove);
            list._isEnumerating = false;
        }
    }
}

internal static class CachedListFactory {
    public static CachedList<T> Create<T>(ReadOnlySpan<T> items) {
        var list = new CachedList<T>();
        foreach (var item in items) {
            list.Add(item);
        }
        return list;
    }
}