using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using AlloyClient.Utils;

namespace AlloyClient.Ui.Components.Elements;

public class FameStar : Sprite {

    public FameStar(int size, int numStars) {
        var starSize = size - 6;
        
        var background = new Ellipse(new EllipseConfig {
            DiameterX = size,
            DiameterY = size,
        });
        AddChild(background);
        
        // todo?: replace with msdfa texture, and add it back as ui render option
        var star = new ObjectRect(new ObjectRectConfig {
            X = size / 2,
            Y = size / 2 - 1,
            Texture = TextureHelper.FromUiAtlas("CharacterList/StarGraphic", 0, false),
            Width = starSize,
            Height = starSize,
            Anchor = UiAnchor.Middle
        });
        
        star.ColorTransformation = FameUtils.StarsToColor(numStars);
        AddChild(star);
    }
}