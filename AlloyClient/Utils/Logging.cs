using System;
using System.IO;
using System.Runtime.CompilerServices;
using AlloyClient.Networking.Packets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

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

public sealed class SingleLineConsoleFormatter(IOptions<ConsoleFormatterOptions> options) : ConsoleFormatter(FormatterName) {
    public const string FormatterName = "alloySingleline";

    private const string Ansi = "\e[";
    private const string AnsiStop = "m";
    private const string Reset = $"{Ansi}0{AnsiStop}";
    private const string Background = "40"; // Black

    private const string FontNorm = "0;";
    private const string FontBold = "1;";

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter) {
        var timestamp = DateTimeOffset.Now.ToString("HH:mm:ss.ffff");
        var level = logEntry.LogLevel switch {
            LogLevel.Trace       => $"{Ansi}{FontNorm}90;{Background}{AnsiStop}TRACE{Reset}",
            LogLevel.Debug       => $"{Ansi}{FontNorm}34;{Background}{AnsiStop}DEBUG{Reset}",
            LogLevel.Information => $"{Ansi}{FontNorm}32;{Background}{AnsiStop}INFO{Reset} ",
            LogLevel.Warning     => $"{Ansi}{FontBold}33;{Background}{AnsiStop}WARN{Reset} ",
            LogLevel.Error       => $"{Ansi}{FontBold}31;{Background}{AnsiStop}ERROR{Reset}",
            LogLevel.Critical    => $"{Ansi}{FontBold}35;{Background}{AnsiStop}CRIT{Reset} ",
            _                    => $"{Ansi}{FontNorm}37;{Background}{AnsiStop}NONE{Reset} "
        };

        var message = logEntry.Formatter(logEntry.State, logEntry.Exception);

        textWriter.WriteLine($"[{timestamp}] {level} {logEntry.Category}[{logEntry.EventId.Id}]:    {message}");

        if (logEntry.Exception is not null)
            textWriter.WriteLine(logEntry.Exception);
    }
}