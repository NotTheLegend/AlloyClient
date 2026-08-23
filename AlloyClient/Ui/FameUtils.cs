using System;
using System.Linq;
using Alloy.UiLib.Extra;
using AlloyClient.Assets.Libraries;

namespace AlloyClient.Ui;

public static class FameUtils {

    public static readonly int[] StarFameRequirements = [20, 150, 400, 800, 2000];
    
    public static int ClassCount => ObjectLibrary.TypeToClassProps.Count;

    public static int MaxStars => ClassCount * StarFameRequirements.Length;

    public static int FameToStar(int fame) {
        var star = 0;
        while (star < StarFameRequirements.Length && fame >= StarFameRequirements[star]) {
            star++;
        }
        
        return star;
    }

    public static int NextStarFame(int bestFame, int currentFame) {
        var fame = Math.Max(bestFame, currentFame);
        return StarFameRequirements.FirstOrDefault(s => s > fame, -1);
    }

    public static ColorTransform StarsToColor(int numStars) {
        var classCount = ClassCount;
        if (classCount == 0 || numStars < classCount) {
            return Transforms.LightBlue;
        }
        
        if (numStars < classCount * 2) {
            return Transforms.DarkBlue;
        }
        
        if (numStars < classCount * 3) {
            return Transforms.Red;
        }
        
        if (numStars < classCount * 4) {
            return Transforms.Orange;
        }
        
        if (numStars < classCount * 5) {
            return Transforms.Yellow;
        }
        
        return Transforms.Default;
    }
}
