using MonoClient.Assets.XmlStructs;

namespace MonoClient.Screens.MapEditor.Misc;

// ReSharper disable InconsistentNaming
public struct Obj {
    public string name;
    public string id;
}

public struct Loc {
    public string ground;
    public Obj[] objs;
    public Obj[] regions;
}

public struct JsonData {
    public byte[] data;
    public int width;
    public int height;
    public Loc[] dict;
}
// ReSharper restore InconsistentNaming

public struct WmapDesc {
    public byte Elevation;
    public string ObjCfg;
    public ObjectProperties ObjDesc;

    public ushort ObjType;
    public TileRegion Region;

    public TerrainType Terrain;
    public GroundProperties TileDesc;
    public ushort TileId;
}

public enum TerrainType {
    None,
    Mountains,
    HighSand,
    HighPlains,
    HighForest,
    MidSand,
    MidPlains,
    MidForest,
    LowSand,
    LowPlains,
    LowForest,
    ShoreSand,
    ShorePlains,
    BeachTowels,
}

public enum TileRegion {
    None = 0,
    Spawn = 1,
    RealmPortals = 2,
    Store1 = 3,
    Store2 = 4,
    Store3 = 5,
    Store4 = 6,
    Store5 = 7,
    Store6 = 8,
    Vault = 9,
    Loot = 10,
    Defender = 11,
    Hallway = 12,
    Enemy = 13,
    Hallway1 = 14,
    Hallway2 = 15,
    Hallway3 = 16,
    Store7 = 17,
    Store8 = 18,
    Store9 = 19,
    GiftingChest = 20,
    Store10 = 21,
    Store11 = 22,
    Store12 = 23,
    Store13 = 24,
    Store14 = 26,
    Store15 = 27,
    Store16 = 28,
    Store17 = 29,
    Store18 = 30,
    Store19 = 31,
    Store20 = 32,
    Store21 = 33,
    Store22 = 34,
    Store23 = 35,
    Store24 = 36,
    GcPortals = 37,
    Slime1 = 38,
    Slime2 = 39,
    Slime3 = 40,
    Slime4 = 41,
    Slime5 = 42,
}