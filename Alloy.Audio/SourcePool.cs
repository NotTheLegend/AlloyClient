namespace Alloy.Audio;

interface IPoolable {
    void Reset();
}

internal class SourcePool<T>(int capacity) where T : class, IPoolable {
    public readonly int Capacity = capacity;
    private readonly T[] _buffer = new T[capacity];
    private int _index = -1;

    public bool HasSource() => _index >= 0;

    public T Pop() {
        return _buffer[_index--];
    }

    public bool TryPop(out T id) {
        if (_index < 0) {
            id = null;
            return false;
        }
        
        id = _buffer[_index--];
        return true;
    }

    public void Push(T source) {
        source.Reset();
        _buffer[++_index] = source;
    }
}