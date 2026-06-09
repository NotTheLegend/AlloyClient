using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Alloy.Engine.Utils;

internal class DisposableTimer : IDisposable {
    public const string Time = "[TIME]";
    
    private readonly ILogger _logger;

    private readonly Stopwatch _sw;
    private readonly string _exitMessage;

    public DisposableTimer(ILogger logger, string entryMessage = null, string exitMessage = "[TIME]") {
        _logger = logger;
        
        if (entryMessage != null) {
            _logger.Log(LogLevel.Trace, entryMessage);
        }
        
        _exitMessage = exitMessage;
        _sw = Stopwatch.StartNew();
    }

    public void Dispose() {
        _logger.Log(LogLevel.Trace, _exitMessage.Replace(Time, _sw.Elapsed.TotalMilliseconds + " ms"));
    }
}