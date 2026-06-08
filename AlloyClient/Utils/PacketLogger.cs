using System;
using System.Runtime.CompilerServices;
using AlloyClient.Networking.Packets;
using AlloyClient.State;
using Microsoft.Extensions.Logging;

namespace AlloyClient.Utils;

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
    private static readonly ILogger Logger = Program.LogFactory.CreateLogger(nameof(PacketLogger));

    public static void LogPacket(IPacket packet) {
        switch (Settings.PacketLogging.Value) {
            case PacketLogLevel.All:
                Logger.Log(LogLevel.Information, $"Packet [{packet.PacketId}] {packet}");
                break;
            case PacketLogLevel.AllNonTick:
                if (!IsTickPacket(packet.PacketId)) {
                    Logger.Log(LogLevel.Information, $"Non-Tick Packet: [{packet.PacketId}] {packet}");
                }

                break;
            case PacketLogLevel.Incoming:
                if (IsIncomingPacket(packet)) {
                    Logger.Log(LogLevel.Information, $"Incoming Packet: [{packet.PacketId}] {packet}");
                }

                break;
            case PacketLogLevel.Outgoing:
                if (IsOutgoingPacket(packet)) {
                    Logger.Log(LogLevel.Information, $"Outgoing Packet: [{packet.PacketId}] {packet}");
                }

                break;
            case PacketLogLevel.IncomingNonTick:
                if (IsIncomingPacket(packet) && !IsTickPacket(packet.PacketId)) {
                    Logger.Log(LogLevel.Information, $"Incoming Non-Tick Packet: [{packet.PacketId}] {packet}");
                }

                break;
            case PacketLogLevel.OutgoingNonTick:
                if (IsOutgoingPacket(packet) && !IsTickPacket(packet.PacketId)) {
                    Logger.Log(LogLevel.Information, $"Outgoing Non-Tick Packet: [{packet.PacketId}] {packet}");
                }

                break;
            case PacketLogLevel.IncomingTick:
                if (IsIncomingPacket(packet) && IsTickPacket(packet.PacketId)) {
                    Logger.Log(LogLevel.Information, $"Incoming Tick Packet: [{packet.PacketId}] {packet}");
                }

                break;
            case PacketLogLevel.OutgoingTick:
                if (IsOutgoingPacket(packet) && IsTickPacket(packet.PacketId)) {
                    Logger.Log(LogLevel.Information, $"Outgoing Tick Packet: [{packet.PacketId}] {packet}");
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