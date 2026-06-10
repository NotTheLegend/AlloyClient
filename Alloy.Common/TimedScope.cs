using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Alloy.Common;

public sealed class TimedScope : IDisposable {
    
    private readonly ILogger _logger;
    private readonly LogLevel _level;
    private readonly Stopwatch _sw;
    private readonly string _exitMessage;

    public TimedScope(ILogger logger, string exitMessage) : this(logger, null, exitMessage) { }
    
    public TimedScope(ILogger logger, string entryMessage, string exitMessage, LogLevel level = LogLevel.Trace) {
        _logger = logger;
        _level = level;
        
        if (entryMessage != null) {
            _logger.Log(_level, entryMessage);
        }
        
        _exitMessage = exitMessage;
        _sw = Stopwatch.StartNew();
    }

    public void Dispose() {
        _logger.Log(_level, _exitMessage, $"{_sw.Elapsed.TotalMilliseconds} ms");
    }
}