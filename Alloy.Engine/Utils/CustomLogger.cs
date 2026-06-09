using Microsoft.Extensions.Logging;

namespace Alloy.Engine.Utils;

public class CustomLogger(ILogger logger, LogLevel minLevel = LogLevel.Debug) : ILogger, OpenTK.Core.Utility.ILogger {

    public OpenTK.Core.Utility.LogLevel Filter { get; set; } = To(minLevel);
    
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) => logger.Log(logLevel, eventId, state, exception, formatter);

    public bool IsEnabled(LogLevel logLevel) => logger.IsEnabled(logLevel);

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => logger.BeginScope(state);

    public void LogInternal(string str, OpenTK.Core.Utility.LogLevel level, string filePath, int line, string member) {
        if (level < Filter) {
            return;
        }
        
        logger.Log(From(level), $"{member} {Path.GetFileName(Path.GetFileName(filePath))}:{line} {str}");
    }

    public void SetFilter(LogLevel level) => Filter = To(level);

    public void Flush() { /* Not needed? ¯\_(ツ)_/¯ */ }

    private static LogLevel From(OpenTK.Core.Utility.LogLevel level) =>
        level switch {
            OpenTK.Core.Utility.LogLevel.Debug => LogLevel.Debug,
            OpenTK.Core.Utility.LogLevel.Info => LogLevel.Information,
            OpenTK.Core.Utility.LogLevel.Warning => LogLevel.Warning,
            OpenTK.Core.Utility.LogLevel.Error => LogLevel.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
    
    private static OpenTK.Core.Utility.LogLevel To(LogLevel level) =>
        level switch {
            LogLevel.Debug => OpenTK.Core.Utility.LogLevel.Debug,
            LogLevel.Information => OpenTK.Core.Utility.LogLevel.Info,
            LogLevel.Warning => OpenTK.Core.Utility.LogLevel.Warning,
            LogLevel.Error => OpenTK.Core.Utility.LogLevel.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
}