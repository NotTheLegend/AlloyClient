using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Alloy.Common;
using Alloy.Engine.Utils;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics;
using OpenTK.Platform;

namespace Alloy.Engine;

public abstract partial class GameWindow {
    
    private static ILogger _logger; 
    
    public readonly WindowHandle Window;
    public readonly OpenGLContextHandle Context;

    protected double TargetFrameTime = 1000d / 60;
    
    private bool _exitFlag;

    protected GameWindow(Version openglVersion, ILoggerFactory logFactory, LogLevel minOpenTkLogLevel = LogLevel.Warning) {
        _logger = logFactory.CreateLogger(nameof(GameWindow));
        
        var options = new ToolkitOptions {
            FeatureFlags = ToolkitFlags.EnableOpenGL,
            Logger = new CustomLogger(logFactory.CreateLogger(nameof(Toolkit)), minOpenTkLogLevel)
        };
        
        Toolkit.Init(options);
        
        var hints = new OpenGLGraphicsApiHints {
            Version = openglVersion
        };

        if (!TryCreateContext(hints, out Window, out Context)) {
            _exitFlag = true;
            return;
        }
        
        Toolkit.OpenGL.SetCurrentContext(Context);
        GLLoader.LoadBindings(Toolkit.OpenGL.GetBindingsContext(Context));
        
        EnableDebugOutput();
        
        Toolkit.Event.EventRaised += HandleEvents;
    }

    protected virtual void Initialize() { }
    
    protected virtual void LoadContent() { }

    protected abstract void Update(GameTime gameTime);
    
    protected abstract void Draw(GameTime gameTime);
    
    protected void Exit() => _exitFlag = true;
    
    protected virtual void Stop() { }

    protected abstract void HandleEvents(EventArgs args);

    public void Run() {
        if (_exitFlag) { // context creation failed
            return;
        }

        using (new TimedScope(_logger, null, "Initialize took {0}")) {
            Initialize();
        }
        
        using (new TimedScope(_logger, null, "LoadContent took {0}")) {
            LoadContent();
        }

        if (OperatingSystem.IsWindows()) {
            SetWindowsJank();
        }
        
        var clockFrequency = 1000d / Stopwatch.Frequency;
        var previousTicks = Stopwatch.GetTimestamp();
        var totalMs = 0d;

        while (true) {
            Toolkit.Window.ProcessEvents(false);

            if (_exitFlag) {
                break;
            }
            
            var currentTicks = Stopwatch.GetTimestamp();
            var deltaMs = (currentTicks - previousTicks) * clockFrequency;
            previousTicks = currentTicks;
            totalMs += deltaMs;
            
            var gameTime = new GameTime(totalMs, deltaMs);
            
            Update(gameTime);
            Draw(gameTime);

            Toolkit.OpenGL.SwapBuffers(Context);
            
            if (TargetFrameTime > 0) {
                var workMs = (Stopwatch.GetTimestamp() - currentTicks) * clockFrequency;
                var remainingMs = TargetFrameTime - workMs;
                if (remainingMs > 0) {
                    OpenTK.Core.Utils.AccurateSleep(remainingMs / 1000.0d, 2);
                }
            }
        }
        
        if (OperatingSystem.IsWindows()) {
            ClearWindowsJank();
        }
        
        Stop();
        
        Toolkit.OpenGL.DestroyContext(Context);
        Toolkit.Window.Destroy(Window);
    }
    
    private static bool TryCreateContext(OpenGLGraphicsApiHints hints, out WindowHandle window, out OpenGLContextHandle context) {
        window = Toolkit.Window.Create(hints);
        context = null;
        
        try {
            context = Toolkit.OpenGL.CreateFromWindow(window);
        } catch {
            Toolkit.Dialog.ShowMessageBox(window, "OpenGL creation failure", $"Application requires a minimum opengl version of {hints.Version.Major}.{hints.Version.Minor}", MessageBoxType.Information);
            return false;
        }

        return true;
    }

    [Conditional("DEBUG")]
    private static void EnableDebugOutput() {
        GL.DebugMessageCallback(OnDebugMessage, nint.Zero);
        GL.DebugMessageControl(DebugSource.DebugSourceApi, DebugType.DebugTypeOther, DebugSeverity.DontCare, 1, [131185], false);
        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);
    }
    
    private static void OnDebugMessage(DebugSource source, DebugType type, uint id, DebugSeverity severity, int length, nint pmessage, nint userParam) {
        var message = Marshal.PtrToStringAnsi(pmessage, length);
        _logger.Log(LogLevel.Warning, "[{0} source={1} type={2} id={3}] {4}", severity, source, type, id, message);
    }

    #region Windows Jank
    
    /* Mix of opentk's GameWindow and improvements from PR #27 */
    
    [LibraryImport("kernel32", SetLastError = true)]
    private static partial IntPtr SetThreadAffinityMask(IntPtr hThread, IntPtr dwThreadAffinityMask);
    
    [LibraryImport("kernel32")]
    private static partial IntPtr GetCurrentThread();

    [LibraryImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
    private static partial uint TimeBeginPeriod(uint ms);

    [LibraryImport("winmm.dll", EntryPoint = "timeEndPeriod")]
    private static partial uint TimeEndPeriod(uint ms);
    
    [SupportedOSPlatform("windows")]
    private void SetWindowsJank() {
        SetThreadAffinityMask(GetCurrentThread(), new IntPtr(1));
        TimeBeginPeriod(1);
    }

    [SupportedOSPlatform("windows")]
    private void ClearWindowsJank() {
        TimeBeginPeriod(1);
    }

    #endregion
}