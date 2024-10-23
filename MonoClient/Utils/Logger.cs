using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MonoClient.Utils;

public enum LogLevel {
    Trace,
    Info,
    Debug,
    Warn,
    Error,
    Fatal
}

// ReSharper disable MethodOverloadWithOptionalParameter
public class Logger {
    private const string DefaultLoggerName = "Logger";

    private static readonly int MaxLevelLength = Enum.GetNames(typeof(LogLevel)).Max(x => x.Length);
    private static readonly object ConsoleLock = new();
    private readonly string _loggerName;

    public Logger(string name) {
        _loggerName = name;
    }

    public Logger(MemberInfo type) : this(type.Name) {
        _loggerName = type.Name;
    }

    public static void Trace(string txt, string loggerName = DefaultLoggerName) {
        Log(txt, LogLevel.Trace, loggerName);
    }

    public static void Info(string txt, string loggerName = DefaultLoggerName) {
        Log(txt, LogLevel.Info, loggerName);
    }

    public static void Debug(string txt, string loggerName = DefaultLoggerName) {
        Log(txt, LogLevel.Debug, loggerName);
    }

    public static void Warn(string txt, string loggerName = DefaultLoggerName) {
        Log(txt, LogLevel.Warn, loggerName);
    }

    public static void Error(string txt, string loggerName = DefaultLoggerName) {
        Log(txt, LogLevel.Error, loggerName);
    }

    public static void Fatal(string txt, string loggerName = DefaultLoggerName) {
        Log(txt, LogLevel.Fatal, loggerName);
    }

    public void Trace(string txt) {
        Log(txt, LogLevel.Trace, _loggerName);
    }

    public void Info(string txt) {
        Log(txt, LogLevel.Info, _loggerName);
    }

    public void Debug(string txt) {
        Log(txt, LogLevel.Debug, _loggerName);
    }

    public void Warn(string txt) {
        Log(txt, LogLevel.Warn, _loggerName);
    }

    public void Error(string txt) {
        Log(txt, LogLevel.Error, _loggerName);
    }

    public void Fatal(string txt) {
        Log(txt, LogLevel.Fatal, _loggerName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Log(string text, LogLevel levelType, string loggerName) {
#if !TRACE
        if (levelType == LogLevel.Trace) {
            return;
        }
#endif
#if !DEBUG
        if (levelType == LogLevel.Debug) {
            return;
        }
#endif

        var levelStr = levelType.ToString().ToUpper().PadRight(MaxLevelLength);
        var loggerStr = loggerName.PadRight(10);
        lock (ConsoleLock) {
            Console.BackgroundColor = GetBackColor(levelType);
            Console.ForegroundColor = GetForeColor(levelType);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {levelStr} {loggerStr} {text}");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ConsoleColor GetBackColor(LogLevel level) {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
            return ConsoleColor.Black;
        }

        switch (level) {
            case LogLevel.Trace:
            case LogLevel.Info:
            case LogLevel.Debug:
            case LogLevel.Warn:
            case LogLevel.Fatal:
                return ConsoleColor.Black;
            case LogLevel.Error:
                return ConsoleColor.Red;
            default:
                throw new ArgumentException($"Invalid LogLevel '{level}'");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ConsoleColor GetForeColor(LogLevel level) {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
            return ConsoleColor.Gray;
        }

        return level switch {
            LogLevel.Trace => ConsoleColor.Gray,
            LogLevel.Info => ConsoleColor.Gray,
            LogLevel.Debug => ConsoleColor.DarkGray,
            LogLevel.Warn => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.White,
            LogLevel.Fatal => ConsoleColor.White,
            _ => throw new ArgumentException($"Invalid LogLevel '{level}'")
        };
    }
}