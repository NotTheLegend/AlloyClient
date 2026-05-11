using System;
using System.Runtime;
using AlloyClient.State;
using Alloy.Common;

namespace AlloyClient;

public static class Program {
    private static readonly Logger Log = new(typeof(Program));

    public static void Main() {
        Log.Info("Starting Game...");
        
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        // CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        Settings.LoadSettings();
        
        var game = new Main();
        game.Run();
        Settings.SaveSettings();
    }
    
    private static void OnProcessExit(object sender, EventArgs e) {
        Settings.SaveSettings();
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
        Settings.SaveSettings();
    }
}