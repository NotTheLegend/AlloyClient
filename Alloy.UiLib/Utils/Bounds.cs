using System;
using Alloy.UiLib.Core;
using OpenTK.Mathematics;

namespace Alloy.UiLib.Utils;

internal record struct Bounds {

    public static readonly Bounds Zero = new(0, 0, 0, 0);

    public int MinX;
    public int MinY;
    public int MaxX;
    public int MaxY;

    public int Width => MaxX - MinX;
    public int Height => MaxY - MinY;

    public int Area => Width * Height;

    public bool HasArea => Area != 0;

    public Bounds(int minX, int minY, int maxX, int maxY) {
        MinX = minX;
        MinY = minY;
        MaxX = maxX;
        MaxY = maxY;
    }

    public Bounds Merge(Bounds bounds) {
        MinX = Math.Min(MinX, bounds.MinX);
        MinY = Math.Min(MinY, bounds.MinY);
        MaxX = Math.Max(MaxX, bounds.MaxX);
        MaxY = Math.Max(MaxY, bounds.MaxY);
        return this;
    }

    public Bounds Scale(Vector2 scale) {
        MinX = (int)(MinX * scale.X);
        MinY = (int)(MinY * scale.Y);
        MaxX = (int)(MaxX * scale.X);
        MaxY = (int)(MaxY * scale.Y);
        return this;
    }

    public Bounds Translate(Vector2i offset) {
        MinX += offset.X;
        MinY += offset.Y;
        MaxX += offset.X;
        MaxY += offset.Y;
        return this;
    }

    public Vector2i Anchor(UiAnchor anchor) {
        var midX = (MinX + MaxX) / 2;
        var midY = (MinY + MaxY) / 2;
        return anchor.Value switch {
            0 /* Default      */ => (0, 0),
            1 /* TopLeft      */ => (MinX, MinY),
            2 /* TopMiddle    */ => (midX, MinY),
            3 /* TopRight     */ => (MaxX, MinY),
            4 /* MiddleLeft   */ => (MinX, midY),
            5 /* Middle       */ => (midX, midY),
            6 /* MiddleRight  */ => (MaxX, midY),
            7 /* BottomLeft   */ => (MinX, MaxY),
            8 /* BottomMiddle */ => (midX, MaxY),
            9 /* BottomRight  */ => (MaxX, MaxY),
            _ /* Missing      */ => (0, 0)
        };
    }

    public static Bounds Merge(Bounds boundsLeft, Bounds boundsRight) => boundsLeft.Merge(boundsRight);

    public static Bounds Scale(Bounds bounds, Vector2 scale) => bounds.Scale(scale);

    public static Bounds Translate(Bounds bounds, Vector2i offset) => bounds.Translate(offset);
    
    public bool Contains(Vector2i position) => position.X >= MinX && position.X <= MaxX && position.Y >= MinY && position.Y <= MaxY;

}