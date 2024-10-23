using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using MonoClient.Networking.Packets;
using MonoClient.Networking.Packets.Outgoing;
using MonoClient.State;
using MonoClient.Utils;

namespace MonoClient.Networking;

public enum ConnectionState {
    Disconnected,
    Connected
}

public static class Client {
    private const int LengthPrefix = 4;

    public static readonly Logger Log = new(typeof(Client));

    private static readonly ConcurrentQueue<IIncomingPacket> IncomingQueue = new();
    private static readonly ConcurrentQueue<IOutgoingPacket> OutgoingQueue = new();

    private static readonly MemoryStream SendStream = new();
    private static readonly NetworkWriter Writer = new(SendStream);

    private static readonly PacketBuffer PacketBuffer = new();

    private static TcpClient _tcpClient;
    private static NetworkStream _netStream;

    public static ConnectionState State;

    public static async void Connect(string ip, ushort port) {
        _tcpClient = new TcpClient();
        _tcpClient.NoDelay = true;

        Log.Info($"Connecting to {ip}:{port}...");

        while (true) {
            try {
                await _tcpClient.ConnectAsync(ip, port);
                break;
            }
            catch (SocketException e) {
                if (e.SocketErrorCode == SocketError.ConnectionRefused) {
                    Log.Warn("Failed to connect to server. Retrying...");

                    await Task.Delay(1000);
                    continue;
                }

                Log.Error(e.ToString());
                return;
            }
        }

        _netStream = _tcpClient.GetStream();

        if (_netStream == null) {
            return;
        }

        State = ConnectionState.Connected;

        Log.Info("Connected to server.");

        SendHello();

        BeginRead(0, LengthPrefix);
    }

    private static void BeginRead(int offset, int amount) {
        _netStream.BeginRead(PacketBuffer.Bytes, offset, amount, RemoteRead, _netStream);
    }

    private static void RemoteRead(IAsyncResult ar) {
        if (State == ConnectionState.Disconnected) {
            return;
        }

        try {
            var read = _netStream.EndRead(ar);
            PacketBuffer.Advance(read);

            if (read == 0) {
                Disconnect("Remote host closed connection.");
                return;
            }

            if (PacketBuffer.Index == LengthPrefix) {
                PacketBuffer.Resize(BitConverter.ToInt32(PacketBuffer.Bytes, 0));
                BeginRead(PacketBuffer.Index, PacketBuffer.BytesRemaining());
            }
            else if (PacketBuffer.BytesRemaining() > 0) {
                BeginRead(PacketBuffer.Index, PacketBuffer.BytesRemaining());
            }
            else {
                var reader = new NetworkReader(new MemoryStream(PacketBuffer.Bytes[LengthPrefix..]));
                var packetId = (PacketId)reader.ReadByte();
                var packet = PacketUtils.CreateIncomingPacket(packetId);
                packet.Read(reader);

                IncomingQueue.Enqueue(packet);

                PacketBuffer.Reset();
                BeginRead(0, LengthPrefix);
            }
        }
        catch (Exception e) {
            Log.Error(e.ToString());
            Disconnect("Error reading from remote host.");
        }
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
        if (State == ConnectionState.Disconnected) {
            return;
        }

        SendStream.SetLength(0);

        while (OutgoingQueue.TryDequeue(out var pkt)) {
            PacketLogger.LogPacket(pkt);

            var pos = (int)SendStream.Position;
            Writer.Write(0);
            Writer.Write((byte)pkt.PacketId);
            pkt.Write(Writer);
            var len = (int)SendStream.Position - pos;
            SendStream.Position = pos;
            Writer.Write(len);
            SendStream.Position = pos;
            SendStream.WriteTo(_netStream);

            pkt.ReturnPacket();
        }
    }

    public static void QueuePacket(IOutgoingPacket pkt) {
        OutgoingQueue.Enqueue(pkt);
    }

    public static void Disconnect(string message = null) {
        if (State != ConnectionState.Disconnected) {
            State = ConnectionState.Disconnected;

            _netStream?.Close();
            _tcpClient?.Close();

            Log.Info($"Disconnecting client {(message != null ? $"({message})" : "")}");

            while (OutgoingQueue.TryDequeue(out var pkt)) {
                pkt.ReturnPacket();
            }

            while (IncomingQueue.TryDequeue(out var pkt)) {
                pkt.ReturnPacket();
            }
        }

        Map.Disconnect();
    }

    private static void SendHello() {
        var hello = Hello.CreatePacket();
        hello.BuildVersion = Settings.BuildVersion;
        hello.GameId = -2;
        hello.GUID = Rsa.EncryptPublic(Data.Account.Email);
        hello.Password = Rsa.EncryptPublic(Data.Account.Password);
        hello.Key = [];
        hello.MapJSON = "";
        hello.Signature = "";
        hello.ClientSize = 420;
        hello.Platform = "mono";
        QueuePacket(hello);
    }
}