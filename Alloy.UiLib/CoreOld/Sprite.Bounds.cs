using OpenTK.Mathematics;

namespace Alloy.UiLib.CoreOld;

public partial class Sprite {

    public bool IsInBounds(Vector2i pos) {
        if (pos.X < _scissor.X || pos.X > _scissor.Z || pos.Y < _scissor.Y || pos.Y > _scissor.W)
            return false;

        return true;
    }
}