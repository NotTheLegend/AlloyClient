namespace Alloy.Engine.Graphics;

public sealed class GpuTimer : IDisposable {

    private const int QueryBufferCount = 4;

    public double LastMilliseconds;

    private readonly int[] _queries = new int[QueryBufferCount];

    private int _writeIndex;
    private int _readIndex;
    private int _pendingCount;
    private bool _active;
    private bool _disposed;

    public GpuTimer() {
        GL.CreateQueries(QueryTarget.TimeElapsed, QueryBufferCount, _queries);
    }

    public void Begin() {
        Poll();

        if (_pendingCount == QueryBufferCount) {
            _active = false;
            return;
        }

        GL.BeginQuery(QueryTarget.TimeElapsed, _queries[_writeIndex]);
        _active = true;
    }

    public void End() {
        if (!_active) {
            return;
        }

        GL.EndQuery(QueryTarget.TimeElapsed);
        _pendingCount++;
        _writeIndex = (_writeIndex + 1) % QueryBufferCount;
        _active = false;
    }

    public void Poll() {
        while (_pendingCount > 0) {
            var available = GL.GetQueryObjecti(_queries[_readIndex], QueryObjectParameterName.QueryResultAvailable);
            if (available == 0) {
                return;
            }

            var nanoseconds = GL.GetQueryObjectui64(_queries[_readIndex], QueryObjectParameterName.QueryResult);
            LastMilliseconds = nanoseconds / 1_000_000d;
            _pendingCount--;
            _readIndex = (_readIndex + 1) % QueryBufferCount;
        }
    }

    public void Dispose() {
        if (_disposed) {
            return;
        }

        GL.DeleteQueries(QueryBufferCount, _queries);
        _disposed = true;
    }
}
