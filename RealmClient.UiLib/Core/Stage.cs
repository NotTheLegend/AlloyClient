using Common;
using RealmClient.UiLib.Extra;
using RealmClient.UiLib.Input;

namespace RealmClient.UiLib.Core;

/// <summary>
/// This is the layer zero sprite that provides access to sprites internal Update/Draw functions, there can only be one
/// </summary>
public class Stage : Sprite {
    
    public int StageWidth { get; internal set; }
    
    public int StageHeight { get; internal set; }

    internal Stage() {
        MouseEnabled = true;
        Stage = this;
    }
    
    public void Update(GameTime gameTime) {
        GTween.Update(gameTime);
        Timer.Update(gameTime);
        MouseInput.Update();
        InternalUpdateLoop(gameTime);
    }

    public void Draw(GameTime gameTime) {
        InternalDrawLoop();
    }
}