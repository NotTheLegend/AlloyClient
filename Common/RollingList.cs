using System.Collections;

namespace Common;

public sealed class RollingList<T>(uint capacity) : IEnumerable<T> {
    
    public int Count { get; private set; }

    private readonly List<T> _items = new((int)capacity);

    private int _offset;

    public T this[int index] {
        get {
            if ((uint) index >= capacity) {
                throw new IndexOutOfRangeException("Index must be less than capacity");
            }
            return _items[(index + _offset) % Count];
        }
    }

    public void Add(T item) {
        if (Count < capacity) {
            _items.Add(item);
            Count++;
            return;
        }

        _items[_offset] = item;
        
        if (++_offset == capacity) {
            _offset = 0;
        }
    }

    public void Clear() {
        _items.Clear();
        Count = 0;
        _offset = 0;
    }

    public IEnumerator<T> GetEnumerator() => new RollingEnumerator<T>(_items, _offset);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private class RollingEnumerator<T1>(List<T1> data, int offset) : IEnumerator<T1> {
        private int _position = -1;

        public bool MoveNext() {
            _position++;
            return _position < data.Count;
        }

        public void Reset() {
            _position = -1;
        }

        private T1 GetCurrent() {
            var index = (_position + offset) % data.Count;
            return data[index];
        }

        object IEnumerator.Current => Current;

        public T1 Current => GetCurrent();

        public void Dispose() { }
    }
}