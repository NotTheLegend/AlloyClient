using System;
using System.Buffers;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Common;

namespace AlloyClient.Networking;

public class SocketReceiveState : IDisposable
{
    private byte[] _buffer;
    private int _bytesAvailable;
    private int _bytesRead;
    private const int BUFFER_SIZE = 0x20000;

    private readonly MemoryStream _stream;
    private readonly NetworkReader _reader;
    
    public SocketReceiveState()
    {
        // Rent memory from the shared pool
        _buffer = ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
        _stream = new MemoryStream(_buffer);
        _reader = new NetworkReader(_stream);
    }

    public void Reset() {
        _bytesAvailable = 0;
        _bytesRead = 0;
    }
    
    public void PrepareSAEA(SocketAsyncEventArgs args) {
        if (_bytesRead > 0) {
            if (_bytesAvailable > 0)
                Buffer.BlockCopy(_buffer, _bytesRead, _buffer, 0, _bytesAvailable);
            _bytesRead = 0;
        }
        Logger.Debug($"Preparing read: {_bytesRead} bytes read | {_bytesAvailable} available | {_buffer.Length} max");
        args.SetBuffer(_buffer, _bytesAvailable, _buffer.Length - _bytesAvailable);
    }

    public void OnDataReceived(int count) {
        _bytesAvailable += count; // Total bytes pending to read
    }

    public bool TryReadPacket(out (byte, NetworkReader) ret)
    {
        ret = default;
        if (_bytesAvailable < 4)
            return false;

        _stream.Seek(_bytesRead, SeekOrigin.Begin); // Make sure we are at the next packet position
        
        int length = _reader.ReadInt32();
        
        if (length < 5 || length > BUFFER_SIZE)
            throw new InvalidDataException($"Invalid packet length: {length}");

        if (length > _bytesAvailable)
            return false;

        var packetId = _reader.ReadByte();
        
        ret.Item2 = _reader;

        ret.Item1 = packetId;

        _bytesAvailable -= length;
        _bytesRead += length;
        if (_bytesAvailable == 0)
            _bytesRead = 0;
        
        return true;
    }

    public void Dispose()
    {
        // Return the buffer to the pool for other connections to use
        var buf = Interlocked.Exchange(ref _buffer, null);
        if (buf != null)
            ArrayPool<byte>.Shared.Return(buf);
        _stream.Dispose();
        _reader.Dispose();
    }
}