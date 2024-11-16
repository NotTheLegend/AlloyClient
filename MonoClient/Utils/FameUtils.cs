using System;
using System.Linq;
using MonoClient.Assets.Libraries;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;

namespace MonoClient.Utils;

public static class FameUtils {

    public static readonly int[] StarFameRequirements = [20, 150, 400, 800, 2000];
    
    public static int ClassCount => ObjectLibrary.TypeToClassProps.Count;
    public static int MaxStars => ClassCount * StarFameRequirements.Length;

    public static int FameToStar(int fame) {
        int star = 0;
        while (star < StarFameRequirements.Length && fame >= StarFameRequirements[star]) {
            star++;
        }
        return star;
    }

    public static int NextStarFame(int bestFame, int currentFame) {
        int fame = Math.Max(bestFame, currentFame);
        return StarFameRequirements.FirstOrDefault(s => s > fame, -1);
    }

    public static ColorTransform StarsToColorTransform(int numStars) {
        if (numStars < ClassCount)
            return ColorTransform.LightBlue;
        if (numStars < ClassCount * 2)
            return ColorTransform.DarkBlue;
        if (numStars < ClassCount * 3)
            return ColorTransform.Red;
        if (numStars < ClassCount * 4)
            return ColorTransform.Orange;
        if (numStars < ClassCount * 5)
            return ColorTransform.Yellow;
        return ColorTransform.Default;
    }
    
    public static Sprite StarsToIcon(int numStars) {
        var bg = new ObjectRect(new ObjectRectConfig {
            Texture = TextureInfo.FromUiAtlas("BlackCircle"),
            Width = 18,
            Height = 18,
        });
        bg.ColorTransformation = ColorTransform.HalfTransparent;
        var star = new ObjectRect(new ObjectRectConfig {
            Texture = TextureInfo.FromUiAtlas("CharacterList/StarGraphic"),
            Width = 16,
            Height = 16,
            Anchor = UiAnchor.Middle,
            X = bg.Width / 2,
            Y = bg.Height / 2,
        });
        bg.AddChild(star);
        star.ColorTransformation = StarsToColorTransform(numStars);
        return bg;
    }
}