using System.Collections.Generic;
using AlloyClient.Ui.Components.Dialogs;
using Alloy.UiLib.Core;
using Alloy.UiLib.Extra;

namespace AlloyClient.Display;

public sealed class DialogManager : Sprite {

    private readonly static Queue<Dialog> Dialogs = [];
    private static Dialog _current;

    public DialogManager() {
        AddEventListener(Event.EnterFrame, OnFrameEnter);
    }

    public static void Enqueue(Dialog dialog) => Dialogs.Enqueue(dialog);

    private void OnFrameEnter() {
        if (_current == null && !TryStart()) return;
        if (_current!.State == DialogState.Closed) OnClosed();
    }

    private bool TryStart() {
        if (!Dialogs.TryDequeue(out var dialog)) return false;
        
        _current = dialog;
        _current.Alpha = 0f;
        AddChild(_current);
        GTween.Add(Tween.New(_current, Easing.SineInOut, 250, 1f, EaseType.Alpha));
        return true;
    }

    private void OnClosed() {
        _current.State = DialogState.Finished;
        GTween.Add(Tween.New(_current, Easing.SineInOut, 250, 0f, EaseType.Alpha, onFinish: () => {
            RemoveChild(_current);
            _current = null;
        }));
    }

}