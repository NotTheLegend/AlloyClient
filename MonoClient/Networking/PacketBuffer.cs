using System;

namespace MonoClient.Networking;

public class PacketBuffer {
    private const int MaxBufferSize = 1048576;

    public byte[] Bytes = new byte[4];
    public int Index;

    public void Resize(int newSize) {
        if (newSize > MaxBufferSize) {
            throw new ArgumentException("New buffer size is too large");
        }

        var old = Bytes;
        Bytes = new byte[newSize];
        Bytes[0] = old[0];
        Bytes[1] = old[1];
        Bytes[2] = old[2];
        Bytes[3] = old[3];
    }

    public void Advance(int numBytes) {
        Index += numBytes;
    }

    public void Reset() {
        Bytes = new byte[4];
        Index = 0;
    }

    public int BytesRemaining() {
        return Bytes.Length - Index;
    }
}