using System;
using System.Collections.Generic;
using Common;
using Common.Atlas;
using Microsoft.Xna.Framework;

namespace MonoClient.UiLib.Data;

public record SliceData(AtlasData AtlasData, Vector2 Cuts);

public static class SliceDataManager {
    
    private static readonly Dictionary<string, SliceData> Slices = new();

    static SliceDataManager() {
        CreateSlice("textBox", 2, 2, "textBox");
    }

    internal static SliceData GetSlice(string lookup) {
        if (!Slices.TryGetValue(lookup, out var slice)) throw new Exception($"Unable to find data for lookup: {lookup}");
        return slice;
    }
    
    public static void CreateSlice(string sliceLookup, int x, int y, string atlasLookup, int lookupIndex = 0) {
        if (Slices.ContainsKey(sliceLookup)) throw new Exception($"Already contains data for lookup: {sliceLookup}");
        
        var data = UiRender.UiAtlas.GetAtlasData(atlasLookup, lookupIndex);
        var cuts = new Vector2(x / AtlasConfig.AtlasWidth, y / AtlasConfig.AtlasHeight);
        Slices[sliceLookup] = new SliceData(data, cuts);
    }
    
}