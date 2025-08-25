using Common;
using OpenTK.Mathematics;
using RealmClient.UiLib.Extra;
using RealmClient.UiLib.Input;

namespace RealmClient.UiLib.Core;

/// <summary>
/// This is the layer zero sprite that provides access to sprites internal Update/Draw functions, there can only be one
/// </summary>
public class Stage : Sprite {

    public static Vector2 ScreenScale;
    
    public int StageWidth { get; private set; }
    
    public int StageHeight { get; private set; }

    internal Stage() {
        MouseEnabled = true;
        Stage = this;
    }

    internal void SetSize(Vector2i dim) {
        StageWidth = dim.X;
        StageHeight = dim.Y;
        SetBaseDimensions(dim.X, dim.Y);
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