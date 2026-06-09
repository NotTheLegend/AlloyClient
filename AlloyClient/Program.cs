using System;
using System.Globalization;
using System.Runtime;
using AlloyClient.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

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