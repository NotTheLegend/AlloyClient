using System.Collections.Generic;
using AlloyClient.Game.Objects;
using Alloy.UiLib.Core;
using Alloy.Common;

namespace AlloyClient.Ui.Character;

public class NotificationLayer : Sprite {
    private static readonly Queue<CharacterStatusText> TextQueue = new();

    public NotificationLayer() {
        AddEventListener(Event.EnterFrame, OnFrameEnter);
    }
    
    public static void AddStatusText(Entity en, string text, uint color, int lifetime, int offsetTime) {
        var data = new CharacterStatusText(en, text, color, lifetime, offsetTime);
        TextQueue.Enqueue(data);
    }

    public void Update(GameTime gameTime) {
        while (TextQueue.TryDequeue(out var characterStatusText)) {
            AddChild(characterStatusText);
        }
    }

    private void OnFrameEnter() {
        while (TextQueue.TryDequeue(out var characterStatusText)) {
            AddChild(characterStatusText);
        }
    }
}