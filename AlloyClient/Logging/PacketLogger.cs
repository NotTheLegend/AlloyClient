using System;
using System.Runtime.CompilerServices;
using AlloyClient.Networking.Packets;
using Microsoft.Extensions.Logging;

namespace AlloyClient.Logging;

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
    private readonly static ILogger Logger = ILogger.CreateLogger(nameof(PacketLogger));

    public static void LogPacket(IPacket packet) {
        switch (Settings.PacketLogging.Value) {
            case PacketLogLevel.All:
                Logger.Log(LogLevel.Information, $"Packet [{packet.PacketId}] {packet}");
                break;
            case PacketLogLevel.AllNonTick when !IsTickPacket(packet.PacketId):
                Logger.Log(LogLevel.Information, $"Non-Tick Packet: [{packet.PacketId}] {packet}");
                break;
            case PacketLogLevel.Incoming when IsIncomingPacket(packet):
                Logger.Log(LogLevel.Information, $"Incoming Packet: [{packet.PacketId}] {packet}");
                break;
            case PacketLogLevel.Outgoing when IsOutgoingPacket(packet):
                Logger.Log(LogLevel.Information, $"Outgoing Packet: [{packet.PacketId}] {packet}");
                break;
            case PacketLogLevel.IncomingNonTick when IsIncomingPacket(packet) && !IsTickPacket(packet.PacketId):
                Logger.Log(LogLevel.Information, $"Incoming Non-Tick Packet: [{packet.PacketId}] {packet}");
                break;
            case PacketLogLevel.OutgoingNonTick when IsOutgoingPacket(packet) && !IsTickPacket(packet.PacketId):
                Logger.Log(LogLevel.Information, $"Outgoing Non-Tick Packet: [{packet.PacketId}] {packet}");
                break;
            case PacketLogLevel.IncomingTick when IsIncomingPacket(packet) && IsTickPacket(packet.PacketId):
                Logger.Log(LogLevel.Information, $"Incoming Tick Packet: [{packet.PacketId}] {packet}");
                break;
            case PacketLogLevel.OutgoingTick when IsOutgoingPacket(packet) && IsTickPacket(packet.PacketId):
                Logger.Log(LogLevel.Information, $"Outgoing Tick Packet: [{packet.PacketId}] {packet}");
                break;
            case PacketLogLevel.Off: break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsIncomingPacket(IPacket packet) => packet is IIncomingPacket;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsOutgoingPacket(IPacket packet) => packet is IOutgoingPacket;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsTickPacket(PacketId packetId) =>
        packetId switch {
            PacketId.Move or PacketId.NewTick or PacketId.Update => true,
            _ => false
        };
}