using System;
using System.Buffers;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using AlloyClient.Networking.Packets;

namespace AlloyClient.Networking;

public class SocketSendState : IDisposable
{
    private byte[] _writeBuffer;
    private byte[] _sendBuffer;

    private int _writeLength;
    private int _sendLength;
    private int _sendOffset;
    private bool _pending;
    
    public SocketSendState()
    {
        _sendBuffer = ArrayPool<byte>.Shared.Rent(0x10000);
        Array.Clear(_sendBuffer);
        _writeBuffer = ArrayPool<byte>.Shared.Rent(0x10000);
        Array.Clear(_writeBuffer);
    }

    public void Reset()
    {
        _writeLength = 0;
        _sendLength = 0;
        _sendOffset = 0;
        _pending = false;
    }

    public void WritePacket(IOutgoingPacket pkt, byte pktId)
    {
        lock (this)
        {
            int start = _writeLength;

            var span = _writeBuffer.AsSpan();

            int bodyStart = start + 5;

            var writer = new SpanWriter(span); // assume you have or can make one
            writer.Position = bodyStart;

            pkt.Write(ref writer);

            int totalLen = writer.Position - start;

            writer.Position = start;
            writer.Write(totalLen);
            writer.Write(pktId);

            _writeLength += totalLen;
        }
    }
    
    public bool TryBeginSend(SocketAsyncEventArgs args) {
        lock (this) {
            if (_pending)
                return false;

            if (_writeLength == 0)
            {
                _pending = false;
                return false;
            }
            
            _pending = true;
            
            var tmp = _sendBuffer;
            _sendBuffer = _writeBuffer;
            _writeBuffer = tmp;

            _sendLength = _writeLength;
            _writeLength = 0;
            _sendOffset = 0;

            args.SetBuffer(_sendBuffer, 0, _sendLength);
        }

        return true;
    }

    public bool OnDataSent(SocketAsyncEventArgs args) // If all bytes were transferred, lastvalidindex will be 0 again
    {
        lock (this)
        {
            _sendOffset += args.BytesTransferred;

            if (_sendOffset < _sendLength)
            {
                // continue sending remaining bytes
                args.SetBuffer(_sendBuffer, _sendOffset, _sendLength - _sendOffset);
                return true; // continue send
            }

            // done
            _sendLength = 0;
            _sendOffset = 0;

            _pending = false;
            return false;
        }
    }
    
    public void Dispose()
    {
        // Return the buffer to the pool for other connections to use
        var buf = Interlocked.Exchange(ref _sendBuffer, null);
        if (buf != null)
            ArrayPool<byte>.Shared.Return(buf);
        buf = Interlocked.Exchange(ref _writeBuffer, null);
        if (buf != null)
            ArrayPool<byte>.Shared.Return(buf);
    }
}