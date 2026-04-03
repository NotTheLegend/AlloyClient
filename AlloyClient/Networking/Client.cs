using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using AlloyClient.Data;
using AlloyClient.Display;
using AlloyClient.Game;
using AlloyClient.Networking.Packets;
using AlloyClient.Networking.Packets.Outgoing;
using AlloyClient.Screens;
using AlloyClient.State;
using AlloyClient.Utils;
using AlloyClient.Models;
using Common;

namespace AlloyClient.Networking;

public enum ConnectionState {
    Disconnected,
    Connected
}

public static class Client {
    public const int RECV_BUFFER_SIZE = 0x40000;
    public const int SEND_BUFFER_SIZE = 0x10000;

    public static readonly Logger Log = new(typeof(Client));

    private static readonly ConcurrentQueue<IIncomingPacket> IncomingQueue = new();

    public static ConnectionState State;

    public static bool IsReconnecting;

    private static readonly SocketAsyncEventArgs _receiveSAEA;
    private static readonly SocketReceiveState _receiveState;
    private static readonly SocketAsyncEventArgs _sendSAEA;
    private static readonly SocketSendState _sendState;
    
    private static Socket _socket;
    private static TcpClient _tcp;

    static Client() {
        _sendState = new SocketSendState();
        _receiveState = new SocketReceiveState();

        _sendSAEA = new SocketAsyncEventArgs();
        _sendSAEA.Completed += ProcessSend;

        _receiveSAEA = new SocketAsyncEventArgs();
        _receiveSAEA.Completed += ProcessReceive;
    }

    private static void Reset() {
        _sendState.Reset();
        _receiveState.Reset();
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

        Task.Run(ReceiveLoop);
    }

    private static void ReceiveLoop()
    {
        while (true) {
            if (State == ConnectionState.Disconnected || !_socket.Connected) {
                Disconnect("Unknown");
                return;
            }

            _receiveState.PrepareSAEA(_receiveSAEA);

            if (_socket.ReceiveAsync(_receiveSAEA))
                break;

            if (!HandleReceive(_receiveSAEA))
                break;
        }
    }

    private static void ProcessReceive(object sender, SocketAsyncEventArgs args)
    {
        if (HandleReceive(args))
            ReceiveLoop();
    }
    
    private static bool HandleReceive(SocketAsyncEventArgs args) {
        if (State == ConnectionState.Disconnected || !_socket.Connected) {
            Disconnect("Unknown");
            return false;
        }

        // Check for any errors during the operation
        var error = args.SocketError;
        if (error != SocketError.Success && error != SocketError.IOPending) {
            string msg = null;
            if (error != SocketError.ConnectionReset) {
                msg = $"Receive SocketError.{error}";
            }

            Disconnect(msg);
            return false;
        }

        if (args.BytesTransferred == 0) {
            Disconnect("Remote host closed connection.");
            return false;
        }
        
        _receiveState.OnDataReceived(args.BytesTransferred);

        while (_receiveState.TryReadPacket(out var result))
        {
            var pktId = (PacketId)result.Item1;
            try {
                // Log.Debug($"RECEIVING {pktId}");
                var pkt = PacketUtils.CreateIncomingPacket(pktId);
                pkt.Read(result.Item2);
                IncomingQueue.Enqueue(pkt);
            }
            catch (Exception ex)
            {
                Log.Error($"Error handling message {pktId}: {ex.Message}");
            }
        }

        return true;
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
        if (State == ConnectionState.Disconnected || !_socket.Connected) {
            return;
        }

        if (_sendState.BeginSend()) {
            _sendState.PrepareSAEA(_sendSAEA);

            if (!_socket.SendAsync(_sendSAEA))
                ProcessSend(null, _sendSAEA);
        }
    }

    private static void ProcessSend(object sender, SocketAsyncEventArgs args) {
        if (State == ConnectionState.Disconnected || !_socket.Connected) {
            Disconnect("Unknown");
            return;
        }

        if (args.SocketError != SocketError.Success)
        {
            Disconnect($"Send Error: {args.SocketError}");
            return;
        }

        _sendState.OnDataSent(args.BytesTransferred);
    }

    public static void QueuePacket(IOutgoingPacket pkt) {
        if (pkt.PacketId == PacketId.Unknown)
            return;
        
        lock (_sendState) {
            _sendState.WritePacket(pkt, (byte)pkt.PacketId);
            // Log.Debug($"SENDING {pkt.PacketId}");
        }
    }

    public static void Disconnect(string message = null) {
        if (State != ConnectionState.Disconnected) {
            State = ConnectionState.Disconnected;

            Reset();

            _tcp?.Close();
            _socket?.Close();

            Log.Info($"Disconnecting client {(message != null ? $"({message})" : "")}");

            while (IncomingQueue.TryDequeue(out var pkt)) {
                pkt.ReturnPacket();
            }
        }
        
        Sound.Music.Stop();
        Map.Reset();
        ScreenManager.FadeTo(new CharacterListScreen());
    }

    private static void SendHello() {
        var login = GlobalData.Get<LoginData>();
        var hello = Hello.CreatePacket();
        hello.BuildVersion = Settings.BuildVersion;
        hello.GameId = -1;
        hello.Username = login.Username;
        hello.Password = login.Password;
        hello.MapJSON = "";
        QueuePacket(hello);
    }
}