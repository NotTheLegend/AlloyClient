using System;
using System.Globalization;
using System.Runtime;
using AlloyClient.Logging;
using Microsoft.Extensions.Logging;

namespace AlloyClient;

public static class Program {
    
    private readonly static ILogger Log = ILogger.CreateLogger(nameof(Program));

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