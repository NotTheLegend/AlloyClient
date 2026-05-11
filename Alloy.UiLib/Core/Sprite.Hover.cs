using System;
using Alloy.UiLib.Input;

namespace Alloy.UiLib.Core;

public partial class Sprite {
    
    internal static Sprite HighestSprite;
    internal static Sprite LastSpriteHovered;

    private static void HandleHover() {
        if (HighestSprite == LastSpriteHovered) 
            return;
        
        HighestSprite?.DispatchEvent(new MouseEvent(MouseEvent.MouseOver, MouseInput.GetMousePosition(), Math.Max(Math.Min(MouseInput.GetVerticalScrollDelta(), 1), -1), KeyboardInput.IsShiftDown(), KeyboardInput.IsCtrlDown(), KeyboardInput.IsAltDown()));
        LastSpriteHovered?.DispatchEvent(new MouseEvent(MouseEvent.MouseOut, MouseInput.GetMousePosition(), Math.Max(Math.Min(MouseInput.GetVerticalScrollDelta(), 1), -1), KeyboardInput.IsShiftDown(), KeyboardInput.IsCtrlDown(), KeyboardInput.IsAltDown()));
        LastSpriteHovered = HighestSprite;
    }

    private void CheckHighestSprite() {
        if (!MouseEnabled || !_canInteract) return;
        if (!IsInBounds(MouseInput.GetMousePosition())) return;

        HighestSprite = this;
    }
}