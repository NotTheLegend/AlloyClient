using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using RealmClient.Display;
using RealmClient.Networking.Packets;
using RealmClient.Networking.Packets.Outgoing;
using RealmClient.Screens.Title;
using RealmClient.State;
using RealmClient.Utils;

namespace RealmClient.Networking;

public enum ConnectionState {
    Disconnected,
    Connected
}

public class SocketSendState {
    public NetworkWriter Writer { get; private set; }
    public MemoryStream Stream { get; private set; }
    public byte[] SocketBuffer { get; private set; } = new byte[Client.SEND_BUFFER_SIZE];
    public int LastValidIndex { get; set; } // End position of the packet that still fits within buffer size
    public int Offset { get; set; }

    public SocketSendState() {
        Stream = new MemoryStream();
        Writer = new NetworkWriter(Stream);
    }

    public int
        PacketBegin() // Returns the start of the packet bytes in the buffer. NEVER use this without locking the SocketSendState instance
    {
        var begin = (int)Stream.Position;
        Stream.Position += 5; // Leave 5 bytes for the header
        return begin;
    }

    public void PacketEnd(int begin, PacketId pkt) {
        var length = (int)Stream.Position - begin;
        Stream.Position -= length; // Write the header [length][id]
        Writer.Write(length);
        Writer.Write((byte)pkt);
        Stream.Position += length - 5; // Go to the next position after packet body to write the next packet

        if (Stream.Position - Offset < Client.SEND_BUFFER_SIZE)
            LastValidIndex = (int)Stream.Position;

        //Logger.Debug($"WRITING {pkt} DATA TO BUFFER");
    }

    public void Reset() {
        lock (this) {
            LastValidIndex = 0;
            Offset = 0;
            Stream.SetLength(0);
        }
    }
}

public static class Client {
    public const int RECV_BUFFER_SIZE = 0x40000;
    public const int SEND_BUFFER_SIZE = 0x10000;

    public static readonly Logger Log = new(typeof(Client));

    private static readonly ConcurrentQueue<IIncomingPacket> IncomingQueue = new();

    public static ConnectionState State;

    public static bool IsReconnecting;

    public static readonly SocketSendState SendState = new();

    private static readonly byte[] _readBuffer = new byte[RECV_BUFFER_SIZE];
    private static readonly NetworkReader _reader = new NetworkReader(new MemoryStream(_readBuffer));
    private static readonly SocketAsyncEventArgs _send = new();
    private static readonly SocketAsyncEventArgs _receive = new();
    private static Socket _socket;
    private static int _bytesRead;
    private static TcpClient _tcp;
    private static bool _pendingSend;

    static Client() {
        _receive.SetBuffer(_readBuffer, 0, RECV_BUFFER_SIZE);
        _receive.Completed += ProcessReceive;
        _send.SetBuffer(SendState.SocketBuffer, 0, SEND_BUFFER_SIZE);
        _send.Completed += ProcessSend;
    }

    private static void Reset() {
        _bytesRead = 0;
        _pendingSend = false;
        _reader.BaseStream.Seek(0, SeekOrigin.Begin);
        SendState.Reset();
    }

    public static async void Connect(string ip, ushort port) {
        Reset();

        _tcp = new TcpClient();
        _tcp.NoDelay = true;

        Log.Info($"Connecting to {ip}:{port}...");

        while (true) {
            try {
                await _tcp.ConnectAsync(ip, port);
                break;
            } catch (SocketException e) {
                if (e.SocketErrorCode == SocketError.ConnectionRefused) {
                    Log.Warn("Failed to connect to server. Retrying...");

                    await Task.Delay(1000);
                    continue;
                }

                Log.Error(e.ToString());
                return;
            }
        }

        _socket = _tcp.Client;
        if (_socket == null) {
            return;
        }

        State = ConnectionState.Connected;

        Log.Info("Connected to server.");

        SendHello();

        StartReceive();
    }

    private static void StartReceive() {
        if (State == ConnectionState.Disconnected || !_socket.Connected) {
            Disconnect("Unknown");
            return;
        }

        // Some exceptions from the SAEA.SetBuffer method and from the Socket.ReceiveAsync() method
        // can't be avoided in certain situations, so a try-catch block is required.
        try {
            _receive.SetBuffer(_bytesRead, RECV_BUFFER_SIZE - _bytesRead);

            if (!_socket.ReceiveAsync(_receive)) {
                ProcessReceive(null, _receive);
            }
        } catch {
            // ignored
        }
    }

    private static void ProcessReceive(object sender, SocketAsyncEventArgs args) {
        if (State == ConnectionState.Disconnected || !_socket.Connected) {
            Disconnect("Unknown");
            return;
        }

        // Check for any errors during the operation
        var error = args.SocketError;
        if (error != SocketError.Success && error != SocketError.IOPending) {
            string msg = null;
            if (error != SocketError.ConnectionReset) {
                msg = $"Receive SocketError.{error}";
            }

            Disconnect(msg);
            return;
        }

        var bytesReceived = args.BytesTransferred;
        if (bytesReceived == 0) {
            Disconnect("Remote host closed connection.");
            return;
        }
        
        // Log.Debug("Reading...");

        // Start reading a new packet
        var bytesNotRead = bytesReceived;
        while (bytesNotRead > 0) {
            var start = (int)_reader.BaseStream.Position;

            var length = _reader.ReadInt32(); // We always start reading from the beginning
            // Log.Debug($"Start: {start} Length: {length} BytesNotRead: {bytesNotRead} BytesRead: {_bytesRead}");
            
            var read = Math.Min(length - _bytesRead, bytesNotRead); // Bytes read in this current iteration
            _bytesRead += read;

            if (_bytesRead < length) // This means we still have bytes to read that we haven't received in this call
            {
                Buffer.BlockCopy(_readBuffer, start, _readBuffer, 0,
                    _bytesRead); // Move this many bytes to the beginning of the buffer
                break;
            }

            var packetId = (PacketId)_reader.ReadByte();
            var pkt = PacketUtils.CreateIncomingPacket(packetId);
            try {
                // Log.Debug($"RECEIVING {packetId} ({_bytesRead} bytes)");
                pkt.Read(_reader);
                IncomingQueue.Enqueue(pkt);
            } catch (Exception e) {
                Log.Error(e.ToString());
            }

            _reader.BaseStream.Seek(start + _bytesRead, SeekOrigin.Begin); // In case we failed processing, go to the end of the packet
            _bytesRead = 0;

            bytesNotRead -= read;
        }

        _reader.BaseStream.Seek(0, SeekOrigin.Begin);

        StartReceive();
    }

    public static void Tick() {
        SendPendingPackets();

        while (IncomingQueue.TryDequeue(out var packet)) {
            PacketLogger.LogPacket(packet);

            packet.Handle();
            packet.ReturnPacket();
        }
    }

    private static void SendPendingPackets() {
        if (State == ConnectionState.Disconnected || _pendingSend) {
            return;
        }

        int count;
        lock (SendState) {
            if (SendState.LastValidIndex == 0) {
                return;
            }

            count = SendState.LastValidIndex - SendState.Offset;

            switch (count) {
                case 0:
                    return;
                case >= SEND_BUFFER_SIZE:
                    Log.Error($"Exceeded buffer size: {count} ({SEND_BUFFER_SIZE})");
                    return;
            }

            var streamBuffer = SendState.Stream.GetBuffer();
            Buffer.BlockCopy(streamBuffer, SendState.Offset, SendState.SocketBuffer, 0, count);

            var pos = (int)SendState.Stream.Position;
            if (SendState.LastValidIndex - pos == 0) // Emptied the stream
            {
                SendState.Stream.Position = 0;
                SendState.LastValidIndex = 0;
                SendState.Offset = 0;
            } else {
                SendState.Offset = SendState.LastValidIndex;
                SendState.LastValidIndex = pos;
            }
        }

        _pendingSend = true;
        _send.SetBuffer(0, count);

        try {
            if (_socket != null && !_socket.SendAsync(_send)) {
                ProcessSend(null, _send);
            }
        } catch {
            // ignored
        }
    }

    private static void ProcessSend(object sender, SocketAsyncEventArgs args) {
        if (State == ConnectionState.Disconnected || !_socket.Connected) {
            Disconnect("Unknown");
            return;
        }

        var error = args.SocketError;
        if (error != SocketError.Success && error != SocketError.IOPending) {
            string msg = null;
            if (error != SocketError.ConnectionReset) {
                msg = $"Send SocketError.{error}";
            }

            Disconnect(msg);
            return;
        }

        _pendingSend = false;
    }

    public static void QueuePacket(IOutgoingPacket pkt) {
        lock (SendState) {
            var begin = SendState.PacketBegin();
            pkt.Write(SendState.Writer);
            SendState.PacketEnd(begin, pkt.PacketId);
            // Log.Debug($"SENDING {pkt.PacketId}");
        }
    }

    public static void Disconnect(string message = null) {
        if (State != ConnectionState.Disconnected) {
            State = ConnectionState.Disconnected;

            Reset();

            _tcp?.Close();
            _socket?.Close();
            _reader.BaseStream.Seek(0, SeekOrigin.Begin);

            Log.Info($"Disconnecting client {(message != null ? $"({message})" : "")}");

            while (IncomingQueue.TryDequeue(out var pkt)) {
                pkt.ReturnPacket();
            }
        }

        Map.Disconnect();
        ScreenManager.FadeTo(new CharacterListScreen());
    }

    private static void SendHello() {
        var hello = Hello.CreatePacket();
        hello.BuildVersion = Settings.BuildVersion;
        hello.GameId = -2;
        hello.GUID = Rsa.EncryptPublic(Data.Account.Email);
        hello.Password = Rsa.EncryptPublic(Data.Account.Password);
        hello.Key = [];
        hello.MapJSON = "";
        QueuePacket(hello);
    }
}