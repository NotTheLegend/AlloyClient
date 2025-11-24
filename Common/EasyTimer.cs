#region

using System;
using System.Diagnostics;

#endregion

namespace Common
{
    public class EasyTimer : IDisposable
    {
        public const string Time = "[TIME]";
        private static readonly Logger Log = new(typeof(EasyTimer));

        private readonly Stopwatch _sw;
        private readonly string _finalMessage;

        public EasyTimer(string firstMessage = null, string finalMessage = "[TIME]")
        {
            if (firstMessage != null)
                Log.Trace(firstMessage);
            _finalMessage = finalMessage;
            _sw = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            Log.Trace(_finalMessage.Replace(Time, _sw.Elapsed.TotalMilliseconds + " ms"));
        }
    }
}