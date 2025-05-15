using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoClient.UiLib.Data;

public record SliceData(AtlasPosition AtlasData, Vector2 Cuts);

public static class SliceLookup {
    
    private static readonly Dictionary<string, SliceData> Slices = new();

    public static bool CheckLookup(string lookup) {
        return Slices.ContainsKey(lookup);
    }

    internal static SliceData GetSlice(string lookup) {
        if (!Slices.TryGetValue(lookup, out var slice)) throw new Exception($"Unable to find data for lookup: {lookup}");
        return slice;
    }

    public static void CreateSlice(string lookup, Vector2 cuts, AtlasPosition uv) {
        Slices[lookup] = new SliceData(uv, cuts);
    }
}