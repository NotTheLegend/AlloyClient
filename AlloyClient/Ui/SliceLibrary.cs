using System;
using Common;
using RealmClient.UiLib.BuiltIn;
using RealmClient.UiLib.Data;
using RealmClient.Utils;
using OpenTK.Mathematics;

namespace RealmClient.Ui;

public static class SliceLibrary {
    
    //todo probably turn this into an xml file instead
    
    public const string StatusBar = "bar3";
    public const string ScrollBarBg = "ScrollBar/ScrollBarBackground";
    public const string ScrollBar = "ScrollBar/ScrollBarHandle";

    public const string TooltipBackgroundLarge = "tooltipBackgroundLarge";
    public const string TooltipBackgroundMedium = "tooltipBackgroundMedium";
    public const string TooltipBackgroundSmall = "tooltipBackgroundSmall";

    public static void Load() {
        CreateSlice(TextInput.BoxLookup, 2, 2, "textBox", false);
        CreateSlice(StatusBar, 7, 7, "bar3");
        
        CreateSlice(ScrollBarBg, 4, 4, "ScrollBar/ScrollBarBackground");
        CreateSlice(ScrollBar, 7, 7, "ScrollBar/ScrollBarHandle");

        CreateSlice(TooltipBackgroundLarge, 30, 30, "tooltipBackgroundLarge");
        CreateSlice(TooltipBackgroundMedium, 20, 20, "tooltipBackgroundMedium");
        CreateSlice(TooltipBackgroundSmall, 10, 10, "tooltipBackgroundSmall");
    }
    
    private static void CreateSlice(string lookup, int x, int y, string atlasLookup, bool padding = true, int lookupIndex = 0) {
        if (SliceLookup.CheckLookup(lookup)) throw new Exception($"Already contains data for lookup: {lookup}");
        
        var uv = Main.UiAtlas.GetAtlasData(atlasLookup, lookupIndex);
        if (!padding) uv.RemovePadding();
        var cuts = new Vector2(x / AtlasConfig.AtlasWidth, y / AtlasConfig.AtlasHeight);
        SliceLookup.CreateSlice(lookup, cuts, uv.ToPosition());
    }
    
}