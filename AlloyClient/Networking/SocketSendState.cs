using System;
using System.IO;
using System.Net.Sockets;
using AlloyClient.Networking.Packets;

namespace AlloyClient.Networking;

public class SocketSendState : IDisposable
{
    private readonly MemoryStream _stream;
    private readonly NetworkWriter _writer;
    private int _bytesWritten;
    private int _pending;
    private int _bytesSent;
    
    public SocketSendState()
    {
        _stream = new MemoryStream(0x20000);
        _writer = new NetworkWriter(_stream);
    }

    public void Reset() {
        _bytesWritten = 0;
        _pending = 0;
        _bytesSent = 0;
    }

    public void WritePacket(IOutgoingPacket pkt, byte pktId)
    {
        lock (this)
        {
            int startPos = _bytesWritten;

            var bodyStart = startPos + 5;
            _stream.Seek(bodyStart, SeekOrigin.Begin);
            pkt.Write(_writer);

            int totalLen = (int)(_stream.Position - startPos);

            _stream.Seek(startPos, SeekOrigin.Begin);
            _writer.Write(totalLen);
            _writer.Write(pktId);

            _bytesWritten += totalLen;
        }
    }

    public bool BeginSend() {
        lock (this) {
            if (_pending != 0 || _bytesWritten == 0)
                return false;
            
            _pending++;
        }

        return true;
    }
    
    public void PrepareSAEA(SocketAsyncEventArgs args)
    {
        lock (this) {
            _bytesSent += _bytesWritten;
            args.SetBuffer(_stream.GetBuffer(), 0, _bytesSent);
        }
    }

    public void OnDataSent(int bytesTransferred) // If all bytes were transferred, lastvalidindex will be 0 again
    {
        lock (this) {
            _pending--;
            var bytesNotSent = _bytesSent - bytesTransferred;
            if (bytesNotSent > 0) {
                var buffer = _stream.GetBuffer();
                Buffer.BlockCopy(buffer, bytesTransferred, buffer, 0, _bytesWritten - bytesTransferred);
            }

            _bytesWritten -= bytesTransferred;
            _bytesSent -= bytesTransferred;
        }
    }
    
    
    public void Dispose()
    {
        _stream.Dispose();
        _writer.Dispose();
    }
}