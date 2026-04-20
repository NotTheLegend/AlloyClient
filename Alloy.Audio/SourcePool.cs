using OpenTK.Audio.OpenAL;

namespace Alloy.Audio;

internal class SourcePool {
    private readonly int _capacity;
    private readonly int[] _buffer;

    private int _index;

    public SourcePool(int capacity) {
        _capacity = capacity;
        _buffer = new int[capacity];
        _index = capacity - 1;
    }

    public bool HasSource() => _index >= 0;

    public int Pop() {
        return _buffer[_index--];
    }

    public void Push(int source) {
        _buffer[++_index] = source;
    }

    public void Initialize() {
        AL.GenSources(_capacity, _buffer.AsSpan());
    }
}