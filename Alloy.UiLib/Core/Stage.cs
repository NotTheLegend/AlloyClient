using Alloy.Common;
using Alloy.Engine;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Extra;
using Alloy.UiLib.Input;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace Alloy.UiLib.Core;

/// <summary>
/// This is the layer zero sprite that provides access to sprites internal Update/Draw functions, there can only be one
/// </summary>
public sealed class Stage : Sprite {

    public static Vector2 ScreenScale;
    
    public int StageWidth { get; private set; }
    
    public int StageHeight { get; private set; }
    
    public static GameTime GameTime { get; private set; } // todo: make not static
    
    public KeyboardState Keyboard => _keyboard;

    private KeyboardState _keyboard;
    private ManualTextInput _manualTextInput;

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
    
    internal void SetKeyDown(Key key, Scancode scancode) {
        if (_keyboard.SetKeyDown(key)) {
            DispatchEvent(new KeyboardEvent(KeyboardEvent.KeyDown, key, scancode, _keyboard.IsCtrlDown(), _keyboard.IsShiftDown(), _keyboard.IsAltDown()));
        }

        if (_manualTextInput.OnManualTextInputDown(key, GameTime.TotalMs)) {
            TextInput.ActiveInput?.OnManualTextInput(key);
        }
    }

    internal void SetKeyUp(Key key, Scancode scancode) {
        if (_keyboard.SetKeyUp(key)) {
            DispatchEvent(new KeyboardEvent(KeyboardEvent.KeyUp, key, scancode, _keyboard.IsCtrlDown(), _keyboard.IsShiftDown(), _keyboard.IsAltDown()));
        }
        
        _manualTextInput.OnManualTextInputUp(key);
    }
}