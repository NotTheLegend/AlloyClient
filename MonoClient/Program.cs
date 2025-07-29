using System;
using MonoClient.State;
using MonoClient.Utils;

namespace MonoClient;

public static class Program {
    private static readonly Logger Log = new(typeof(Program));

    public static Main Game { get; private set; }

    public static void Main() {
        Log.Info("Starting Game...");
        Game = new Main();
        
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        Settings.LoadSettings();
        Game.Run();
    }
    
    private static void OnProcessExit(object sender, EventArgs e) {
        Settings.SaveSettings();
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
        Settings.SaveSettings();
    }
}