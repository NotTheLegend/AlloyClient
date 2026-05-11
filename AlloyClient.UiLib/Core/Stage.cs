using AlloyClient.UiLib.Extra;
using AlloyClient.UiLib.Input;
using Alloy.Common;
using OpenTK.Mathematics;

namespace AlloyClient.UiLib.Core;

/// <summary>
/// This is the layer zero sprite that provides access to sprites internal Update/Draw functions, there can only be one
/// </summary>
public sealed class Stage : Sprite {

    public static Vector2 ScreenScale;
    
    public int StageWidth { get; private set; }
    
    public int StageHeight { get; private set; }
    
    public static GameTime GameTime { get; private set; }

    internal Stage() {
        MouseEnabled = true;
        Stage = this;
    }

    internal void SetSize(Vector2i dim) {
        StageWidth = dim.X;
        StageHeight = dim.Y;
    }
    
    public void Update(GameTime gameTime) {
        GameTime = gameTime;
        GTween.Update(gameTime);
        Timer.Update(gameTime);
        MouseInput.Update();
        InternalUpdateLoop();
    }

    public void Draw(GameTime gameTime) {
        InternalDrawLoop();
    }
}