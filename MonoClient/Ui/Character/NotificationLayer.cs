using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoClient.Objects;
using MonoClient.UiLib.Core;
using MonoClient.Utils;

namespace MonoClient.Ui.Character;

public class NotificationLayer : Sprite {
    private static readonly Queue<CharacterStatusText> _textQueue = new();
    
    public static void AddStatusText(Entity en, string text, uint color, int lifetime, int offsetTime) {
        var data = new CharacterStatusText(en, text, color, lifetime, offsetTime);
        _textQueue.Enqueue(data);
    }

    protected override void OnUpdate(GameTime gameTime) {
        while (_textQueue.TryDequeue(out var characterStatusText)) {
            AddChild(characterStatusText);
        }
    }
}