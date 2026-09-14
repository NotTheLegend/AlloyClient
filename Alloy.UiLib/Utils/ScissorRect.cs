using System;
using OpenTK.Mathematics;

namespace Alloy.UiLib.Utils;

public readonly record struct ScissorRect(int X, int Y, int Width, int Height) {
    public static readonly ScissorRect Default = new(int.MinValue / 2, int.MinValue / 2, int.MaxValue, int.MaxValue);

    internal ScissorRect ToGlobal(Vector2i position, Vector2 scale) => new(X + position.X, Y + position.Y, (int)(Width * scale.X), (int)(Height * scale.Y));

    internal bool Contains(Vector2i position) => position.X > X && position.X < X + Width && position.Y > Y && position.Y < Y + Height;

    public static ScissorRect operator +(ScissorRect left, ScissorRect right) => new(
        Math.Max(left.X, right.X),
        Math.Max(left.Y, right.Y),
        Math.Min(left.Width, right.Width),
        Math.Min(left.Height, right.Height)
    );
    
    public static implicit operator Vector4(ScissorRect rect) => new(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
}