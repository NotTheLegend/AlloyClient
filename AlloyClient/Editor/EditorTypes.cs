using System;

namespace AlloyClient.Editor;

public enum EditorDrawType {
    Ground,
    Objects,
    Regions,
}

public enum EditorToolType {
    Select,
    Pencil,
    Line,
    Shape,
    Bucket,
    Picker,
    Eraser,
    Edit,
}

public sealed class EditorTileData {
    public int GroundType = -1;
    public int ObjectType;
    public string ObjectConfig;
    public int RegionType;
    public int TerrainType;
    public int Elevation;

    public EditorTileData Clone() {
        return new EditorTileData {
            GroundType = GroundType,
            ObjectType = ObjectType,
            ObjectConfig = ObjectConfig,
            RegionType = RegionType,
            TerrainType = TerrainType,
            Elevation = Elevation,
        };
    }

    public void CopyFrom(EditorTileData source) {
        GroundType = source?.GroundType ?? -1;
        ObjectType = source?.ObjectType ?? 0;
        ObjectConfig = source?.ObjectConfig;
        RegionType = source?.RegionType ?? 0;
        TerrainType = source?.TerrainType ?? 0;
        Elevation = source?.Elevation ?? 0;
    }

    public int GetType(EditorDrawType drawType) {
        return drawType switch {
            EditorDrawType.Ground => GroundType,
            EditorDrawType.Objects => ObjectType,
            EditorDrawType.Regions => RegionType,
            _ => 0,
        };
    }

    public bool SameAs(EditorTileData other) {
        return other is not null && GroundType == other.GroundType && ObjectType == other.ObjectType &&
               ObjectConfig == other.ObjectConfig && RegionType == other.RegionType &&
               TerrainType == other.TerrainType && Elevation == other.Elevation;
    }
}

public sealed class EditorSelection {
    public int StartX = -1;
    public int StartY = -1;
    public int EndX = -1;
    public int EndY = -1;

    public bool IsActive() => StartX >= 0;
    public int GetWidth() => IsActive() ? EndX - StartX + 1 : 0;
    public int GetHeight() => IsActive() ? EndY - StartY + 1 : 0;
    public bool Contains(int x, int y) => !IsActive() || x >= StartX && x <= EndX && y >= StartY && y <= EndY;

    public void Set(int startX, int startY, int endX, int endY) {
        StartX = Math.Min(startX, endX);
        StartY = Math.Min(startY, endY);
        EndX = Math.Max(startX, endX);
        EndY = Math.Max(startY, endY);
    }

    public void Clear() {
        StartX = -1;
        StartY = -1;
        EndX = -1;
        EndY = -1;
    }

    public EditorSelection Clone() {
        return new EditorSelection { StartX = StartX, StartY = StartY, EndX = EndX, EndY = EndY };
    }

    public void CopyFrom(EditorSelection selection) {
        if (selection is null || !selection.IsActive()) {
            Clear();
            return;
        }

        StartX = selection.StartX;
        StartY = selection.StartY;
        EndX = selection.EndX;
        EndY = selection.EndY;
    }
}

public sealed class EditorClipboard {
    public int Width;
    public int Height;
    public EditorTileData[] Tiles = [];

    public void Clear() {
        Width = 0;
        Height = 0;
        Tiles = [];
    }

    public void Copy(EditorMapData map, EditorSelection selection) {
        if (!selection.IsActive()) return;

        Width = selection.GetWidth();
        Height = selection.GetHeight();
        Tiles = new EditorTileData[Width * Height];
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
            Tiles[x + y * Width] = map.GetTile(selection.StartX + x, selection.StartY + y).Clone();
    }

    public EditorTileData GetTile(int x, int y) {
        if (x < 0 || y < 0 || x >= Width || y >= Height) return null;

        return Tiles[x + y * Width];
    }
}

public sealed class EditorBrush {
    public const int MaxSize = 32;
    public const int Chance = 25;

    public EditorDrawType DrawType = EditorDrawType.Ground;
    public int GroundType = -1;
    public int ObjectType;
    public int RegionType;
    public int Size;
    public bool Replace = true;

    public int GetSelectedType() {
        return DrawType switch {
            EditorDrawType.Ground => GroundType,
            EditorDrawType.Objects => ObjectType,
            EditorDrawType.Regions => RegionType,
            _ => 0,
        };
    }

    public void SetSelectedType(int type) {
        switch (DrawType) {
            case EditorDrawType.Ground:
                GroundType = type;
                break;
            case EditorDrawType.Objects:
                ObjectType = type;
                break;
            case EditorDrawType.Regions:
                RegionType = type;
                break;
        }
    }
}