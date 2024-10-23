using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoClient.Ui.Components.Dialogs;
using MonoClient.UiLib;
using MonoClient.UiLib.Core.Events.Types;

namespace MonoClient.Display;

public static class DialogManager {

    private static Queue<Dialog> _dialogs = [];
    private static Dialog _current = null;

    public static void Enqueue(Dialog dialog) => _dialogs.Enqueue(dialog);

    public static void Update(GameTime gameTime, ref DisplayState state) {
        if (_current == null && !TryStart()) return;
        if (_current?.State == DialogState.Closed) OnClosed();
        //todo change to match panelManager
        
        _current!.Update(gameTime);
        state = DisplayState.Dialog;
    }

    public static void HandleMouseEvents(ref MouseEventId consumed) {
        _current?.HandleMouseEvents(ref consumed);
    }

    public static void Draw(GameTime gameTime) {
        _current?.Draw(gameTime);
    }

    private static bool TryStart() {
        if (!_dialogs.TryDequeue(out var dialog)) return false;
        
        _current = dialog;
        _current.Alpha = 0f;
        GTween.Add(Tween.New(_current, Easing.SineInOut, 250, 1f, EaseType.Alpha));
        return true;
    }

    private static void OnClosed() {
        _current.State = DialogState.Finished;
        GTween.Add(Tween.New(_current, Easing.SineInOut, 250, 0f, EaseType.Alpha, onFinish: () => _current = null));
    }

}