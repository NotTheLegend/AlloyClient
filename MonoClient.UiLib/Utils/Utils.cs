using System.Threading.Tasks;
using MonoClient.UiLib.Enums;

namespace MonoClient.UiLib.Utils;

internal static class Utils {
    
    internal static TaskState GetStatus(this Task task) {
        if (task.IsFaulted)
            return TaskState.Faulted;
        if (task.IsCanceled)
            return TaskState.Canceled;

        return TaskState.Completed;
    }
    
}