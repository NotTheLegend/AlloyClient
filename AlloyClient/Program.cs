using System;
using System.Globalization;
using System.IO;
using System.Runtime;
using AlloyClient.State;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace AlloyClient;

public static class Program {
    
    public static readonly ILoggerFactory LogFactory = LoggerFactory.Create(builder => builder.AddConsole(options => { options.FormatterName = SingleLineConsoleFormatter.FormatterName; })
            .AddConsoleFormatter<SingleLineConsoleFormatter, ConsoleFormatterOptions>()
#if DEBUG
            .SetMinimumLevel(LogLevel.Trace)
#endif
    );

    private static readonly ILogger Log = LogFactory.CreateLogger(nameof(Program));

    public static void Main() {
        Log.Log(LogLevel.Information, "Starting Game...");
        
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        Settings.LoadSettings();
        
        var game = new Main();
        game.Run();
    }
    
    private static void OnProcessExit(object sender, EventArgs e) {
        Settings.SaveSettings();
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
        Settings.SaveSettings();
    }
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