using System;
using System.Runtime.CompilerServices;
using RealmClient.Networking.Packets;
using RealmClient.State;

namespace RealmClient.Utils;

public enum PacketLogLevel {
    All,
    AllNonTick,
    Incoming,
    Outgoing,
    IncomingNonTick,
    OutgoingNonTick,
    IncomingTick,
    OutgoingTick,
    Off
}

public static class PacketLogger {
    private static readonly Logger Log = new("PacketLog");

    public static void LogPacket(IPacket packet) {
        switch (Settings.PacketLogging.Value) {
            case PacketLogLevel.All:
                Log.Info($"Packet [{packet.PacketId}] {packet}");
                break;
            case PacketLogLevel.AllNonTick:
                if (!IsTickPacket(packet.PacketId)) {
                    Log.Info($"Non-Tick Packet: [{packet.PacketId}] {packet}");
                }

                break;
            case PacketLogLevel.Incoming:
                if (IsIncomingPacket(packet)) {
                    Log.Info($"Incoming Packet: [{packet.PacketId}] {packet}");
                }

                break;
            case PacketLogLevel.Outgoing:
                if (IsOutgoingPacket(packet)) {
                    Log.Info($"Outgoing Packet: [{packet.PacketId}] {packet}");
                }

                break;
            case PacketLogLevel.IncomingNonTick:
                if (IsIncomingPacket(packet) && !IsTickPacket(packet.PacketId)) {
                    Log.Info($"Incoming Non-Tick Packet: [{packet.PacketId}] {packet}");
                }

                break;
            case PacketLogLevel.OutgoingNonTick:
                if (IsOutgoingPacket(packet) && !IsTickPacket(packet.PacketId)) {
                    Log.Info($"Outgoing Non-Tick Packet: [{packet.PacketId}] {packet}");
                }

                break;
            case PacketLogLevel.IncomingTick:
                if (IsIncomingPacket(packet) && IsTickPacket(packet.PacketId)) {
                    Log.Info($"Incoming Tick Packet: [{packet.PacketId}] {packet}");
                }

                break;
            case PacketLogLevel.OutgoingTick:
                if (IsOutgoingPacket(packet) && IsTickPacket(packet.PacketId)) {
                    Log.Info($"Outgoing Tick Packet: [{packet.PacketId}] {packet}");
                }

                break;
            case PacketLogLevel.Off:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsIncomingPacket(IPacket packet) {
        return packet is IIncomingPacket;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsOutgoingPacket(IPacket packet) {
        return packet is IOutgoingPacket;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsTickPacket(PacketId packetId) {
        return packetId switch {
            PacketId.Move => true,
            PacketId.NewTick => true,
            PacketId.Update => true,
            //PacketId.UpdateAck => true,
            //PacketId.Pong => true,
            //PacketId.Ping => true,
            _ => false
        };
    }
}