using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoClient.Ui.Components.Dialogs;
using MonoClient.UiLib;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Core.Events.Types;

namespace MonoClient.Display;

public class DialogManager : Sprite {

    private static readonly Queue<Dialog> Dialogs = [];
    private static Dialog _current;

    public static void Enqueue(Dialog dialog) => Dialogs.Enqueue(dialog);

    protected override void OnUpdate(GameTime gameTime) {
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