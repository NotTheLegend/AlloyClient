using System;
using System.Collections.Generic;
using System.Diagnostics;
using Common;

namespace AlloyClient.UiLib.Extra;

public sealed class Timer {
    
    private const long TicksPerMs = 10000;

    private static readonly Queue<Timer> Queue = [];
    private static readonly List<Timer> Timers = [];

    private readonly Queue<(TimerEvent, Delegate, bool)> _queue = [];
    private readonly List<Delegate> _timerEvents = [];
    private readonly List<Delegate> _timerCompleteEvents = [];
    
    public int CurrentCount { get; private set; }
    
    public int RepeatCount { get; set; }
    
    public double Delay { get; set; }

    private bool _isRunning;

    private long _startTime;

    public Timer(double delay, int repeatCount = 0) {
        Delay = delay;
        RepeatCount = repeatCount;
    }

    public void Start() {
        _isRunning = true;
        _startTime = Stopwatch.GetTimestamp();
        Queue.Enqueue(this);
    }

    public void Stop() {
        _isRunning = false;
    }

    public void Reset() {
        _isRunning = false;
        CurrentCount = 0;
    }

    public static void Update(GameTime gameTime) {
        while (Queue.TryDequeue(out var timer)) {
            Timers.Add(timer);
        }
        
        foreach (var timer in Timers) {
            timer.Tick(gameTime);
        }

        Timers.RemoveAll(t => !t._isRunning);
    }

    private void Tick(GameTime gameTime) {
        if (!_isRunning) return;

        while (_queue.TryDequeue(out var data)) {
            switch (data.Item1) {
                case TimerEvent.Timer when data.Item3:
                    _timerEvents.Add(data.Item2);
                    break;
                case TimerEvent.TimerComplete when data.Item3:
                    _timerCompleteEvents.Add(data.Item2);
                    break;
                case TimerEvent.Timer:
                    _timerEvents.Remove(data.Item2);
                    break;
                case TimerEvent.TimerComplete:
                    _timerCompleteEvents.Remove(data.Item2);
                    break;
            }
        }
        
        var current = Stopwatch.GetTimestamp();
        var dt = (current - _startTime) / TicksPerMs;

        if (dt > Delay) {
            _startTime = current;
            CurrentCount++;
            Handle();
        }
    }

    private void Handle() {
        if (RepeatCount > 0 && CurrentCount >= RepeatCount) {
            foreach (var callback in _timerCompleteEvents) {
                callback.DynamicInvoke();
            }
            _isRunning = false;
        }

        foreach (var callback in _timerEvents) {
            callback.DynamicInvoke();
        }
    }

    public void AddEventListener(TimerEvent timerEvent, Delegate callback) {
        if (callback is not Action) {
            Console.WriteLine("not a valid callback");
            return;
        }
        
        _queue.Enqueue((timerEvent, callback, true));
    }
    
    public void RemoveEventListener(TimerEvent timerEvent, Delegate callback) {
        _queue.Enqueue((timerEvent, callback, false));
    }
}

public enum TimerEvent {
    Timer,
    TimerComplete
}