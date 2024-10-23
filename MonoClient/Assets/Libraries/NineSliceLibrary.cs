using System.Collections.Generic;
using System.Numerics;
using Common;

namespace MonoClient.Assets.Libraries;

public static class NineSliceLibrary {
    public static readonly Dictionary<string, Vector2> Get = new();

    public static void Load() {
        CreateSlice("bar1", 7, 7);
        CreateSlice("bar2", 7, 7);
        CreateSlice("t8", 15, 15);
        CreateSlice("textBox", 2, 2);
        
        CreateSlice("ScrollBar/ScrollBarBackground", 4, 4);
        CreateSlice("ScrollBar/ScrollBarHandle", 7, 7);
    }

    private static void CreateSlice(string lookup, int x, int y) {
        Get[lookup] = new Vector2(x / AtlasConfig.AtlasWidth, y / AtlasConfig.AtlasHeight);
    }
}