using Microsoft.Xna.Framework;
using MonoClient.UiLib.Core.Events.Types;

namespace MonoClient.UiLib.Core;

public abstract class Screen : Sprite {

    public Screen() { }

    public new virtual void Update(GameTime gameTime) {
        Scale = UiRender.ScreenScale;
        base.Update(gameTime);
    }

    public new void HandleMouseEvents(ref MouseEventId consumed) {
        base.HandleMouseEvents(ref consumed);
    }

    public virtual void Draw(GameTime gameTime) {
        InternalDrawLoop();
    }
}