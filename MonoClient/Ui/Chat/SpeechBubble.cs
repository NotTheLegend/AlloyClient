using Microsoft.Xna.Framework;
using MonoClient.Objects;
using MonoClient.UiLib.Core;

namespace MonoClient.Ui.Chat;

public struct SpeechData {

    public Entity Owner;

    public string Text;
}

public sealed class SpeechBubble : Sprite {

    private Entity _owner;

    public SpeechBubble(SpeechData data) {
        _owner = data.Owner;
    }

    protected override void OnUpdate(GameTime gameTime) {
        if (_owner == null) return;

        X = (int)_owner.Position.X;
    }
}