using System;
using RealmClient.State;
using RealmClient.Utils;

namespace RealmClient;

public static class Program {
    private static readonly Logger Log = new(typeof(Program));

    public static void Main() {
        Log.Info("Starting Game...");
        
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

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