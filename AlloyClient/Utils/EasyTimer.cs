#region

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

#endregion

namespace AlloyClient.Utils;

public class EasyTimer : IDisposable {
    public const string Time = "[TIME]";
    private static readonly ILogger Logger = Program.LogFactory.CreateLogger(nameof(EasyTimer));

    private readonly Stopwatch _sw;
    private readonly string _finalMessage;

    public EasyTimer(string firstMessage = null, string finalMessage = "[TIME]") {
        if (firstMessage != null)
            Logger.Log(LogLevel.Trace, firstMessage);
        _finalMessage = finalMessage;
        _sw = Stopwatch.StartNew();
    }

    public void Dispose() {
        Logger.Log(LogLevel.Trace, _finalMessage.Replace(Time, _sw.Elapsed.TotalMilliseconds + " ms"));
    }
}