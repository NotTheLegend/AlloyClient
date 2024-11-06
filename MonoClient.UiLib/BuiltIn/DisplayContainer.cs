using Microsoft.Xna.Framework;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Core.Events.Types;

namespace MonoClient.UiLib.BuiltIn;

public class DisplayContainer : Sprite {
    public virtual void Update(GameTime gameTime) {
        Scale = UiRender.ScreenScale;
        InternalUpdateLoop(gameTime);
    }

    public virtual void Draw(GameTime gameTime) {
        InternalDrawLoop();
    }
}