namespace Alloy.Audio;

internal static class Pools {
    public readonly static SourcePool<int> Sources = new (256);
    public readonly static TrackPool<StaticTrack> StaticTracks = new (64);
    public readonly static TrackPool<StreamTrack> StreamTracks = new (4);
}

internal interface IPoolable;

internal class SourcePool<T>(int capacity) {
    public readonly int Capacity = capacity;
    private readonly T[] _buffer = new T[capacity];
    private int _index = -1;

    public bool TryPop(out T id) {
        if (_index < 0) {
            id = default;
            return false;
        }
        
        id = _buffer[_index--];
        return true;
    }

    public void Push(T source) {
        _buffer[++_index] = source;
    }
}

internal class TrackPool<T>(int initialCapacity) where T : IPoolable, new() {
    private readonly List<T> _buffer = new (initialCapacity);
    private int _index = -1;

    public T Pop() {
        Pop(out var track);
        return track;
    }

    public void Pop(out T track) {
        if (_index < 0) {
            track = new T();
            return;
        }

        track = _buffer[_index];
        _buffer.RemoveAt(_index--);
    }

    public void Push(T item) {
        _buffer.Add(item);
        _index++;
    }
}

