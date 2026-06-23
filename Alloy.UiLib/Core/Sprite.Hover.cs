using Alloy.UiLib.Input;

namespace Alloy.UiLib.Core;

public partial class Sprite {
    
    internal static Sprite HighestSprite;
    internal static Sprite LastSpriteHovered;

    private static void HandleHover(KeyboardState keyboard) {
        if (HighestSprite == LastSpriteHovered) 
            return;

        HighestSprite?.DispatchEvent(new MouseEvent(MouseEvent.MouseOver, MouseInput.GetMousePosition(), MouseInput.GetVerticalScrollDelta(), keyboard.IsShiftDown(), keyboard.IsCtrlDown(), keyboard.IsAltDown()));
        LastSpriteHovered?.DispatchEvent(new MouseEvent(MouseEvent.MouseOut, MouseInput.GetMousePosition(), MouseInput.GetVerticalScrollDelta(), keyboard.IsShiftDown(), keyboard.IsCtrlDown(), keyboard.IsAltDown()));
        LastSpriteHovered = HighestSprite;
    }

    private void CheckHighestSprite() {
        if (!MouseEnabled || !_canInteract) return;
        if (!IsInBounds(MouseInput.GetMousePosition())) return;

        HighestSprite = this;
    }
}