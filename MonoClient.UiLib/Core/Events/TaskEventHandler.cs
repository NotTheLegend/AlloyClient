using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonoClient.UiLib.Core.Events;

internal class TaskEventHandler {
    private readonly List<(TaskEvent, Task, Delegate)> _pendingTasks = [];
    private readonly Queue<(TaskEvent, Task, Delegate)> _completedTasks = new();

    public void Handle() {
        while (_completedTasks.Count > 0) {
            var cb = _completedTasks.Dequeue();
            // If the task was removed before it was completed, skip it
            if (!_pendingTasks.Remove((cb.Item1, cb.Item2, cb.Item3))) {
                continue;
            }

            if (cb.Item2.IsCompleted) {
                switch (cb.Item1) {
                    case TaskEvent.Completed:
                    case TaskEvent.Faulted when cb.Item2.IsFaulted:
                    case TaskEvent.Canceled when cb.Item2.IsCanceled:
                        switch (cb.Item3) {
                            case Action action:
                                action();
                                break;
                            case { } genericAction: {
                                var resultProperty = cb.Item2.GetType().GetProperty("Result");
                                var result = resultProperty?.GetValue(cb.Item2);
                                genericAction.DynamicInvoke(result);
                                break;
                            }
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    public void AddEvent<T>(TaskEvent type, Task<T> task, Action<T> action) {
        _pendingTasks.Add((type, task, action));
        task.ContinueWith(_ => TaskCompleted(type, task, action));
    }

    public void RemoveEvent<T>(TaskEvent type, Task<T> task, Action<T> action) {
        _pendingTasks.Remove((type, task, action));
    }

    public void AddEvent(TaskEvent type, Task task, Action action) {
        _pendingTasks.Add((type, task, action));
        task.ContinueWith(_ => TaskCompleted(type, task, action));
    }

    public void RemoveEvent(TaskEvent type, Task task, Action action) {
        _pendingTasks.Remove((type, task, action));
    }

    private void TaskCompleted(TaskEvent type, Task task, Delegate action) {
        _completedTasks.Enqueue((type, task, action)); // Push to _completedTasks which will get dequeued in the main thread
    }
}

public enum TaskEvent {
    Completed = 0,
    Faulted = 1,
    Canceled = 2
}