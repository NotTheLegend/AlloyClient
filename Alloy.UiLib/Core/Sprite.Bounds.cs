using System;
using Alloy.UiLib.Utils;
using OpenTK.Mathematics;

namespace Alloy.UiLib.Core;

public partial class Sprite {

    public bool IsInBounds(Vector2i pos) {
        if (pos.X < _scissor.X || pos.X > _scissor.Z || pos.Y < _scissor.Y || pos.Y > _scissor.W) {
            return false;
        }

        var localPosition = GlobalToLocal(pos);

        return CollisionType switch {
            CollisionType.Square => SquareHitbox(localPosition),
            CollisionType.Ellipse => EllipseHitbox(localPosition),
            CollisionType.Vertices => ComplexHitbox(localPosition, pos),
            CollisionType.Custom => CustomHitbox(new Vector2i((int)(pos.X - _worldTransform.TX), (int)(pos.Y - _worldTransform.TY))),
            CollisionType.CustomNoScale => CustomHitbox(new Vector2i((int)localPosition.X, (int)localPosition.Y)),
            _ => throw new ArgumentOutOfRangeException($"{CollisionType} not handled in InternalBoundsCheck")
        };
    }

    private bool SquareHitbox(Vector2 pos) {
        return pos.X >= 0f && pos.X <= ContentWidth && pos.Y >= 0f && pos.Y <= ContentHeight;
    }

    private bool EllipseHitbox(Vector2 pos) {
        if (Radii.X <= 0 || Radii.Y <= 0) {
            return false;
        }

        var x = pos.X - Radii.X;
        var y = pos.Y - Radii.Y;
        return x * x / (Radii.X * Radii.X) + y * y / (Radii.Y * Radii.Y) <= 1f;
    }

    private bool ComplexHitbox(Vector2 pos, Vector2i globalPosition) {
        var len = Indices.Length;

        for (var i = 0; i < len; i += 3) {
            var t1 = VertexData[Indices[i + 0]].Position;
            var t2 = VertexData[Indices[i + 1]].Position;
            var t3 = VertexData[Indices[i + 2]].Position;

            var d1 = (pos.X - t2.X) * (t1.Y - t2.Y) - (t1.X - t2.X) * (pos.Y - t2.Y);
            var d2 = (pos.X - t3.X) * (t2.Y - t3.Y) - (t2.X - t3.X) * (pos.Y - t3.Y);
            var d3 = (pos.X - t1.X) * (t3.Y - t1.Y) - (t3.X - t1.X) * (pos.Y - t1.Y);

            if (!((d1 < 0 || d2 < 0 || d3 < 0) && (d1 > 0 || d2 > 0 || d3 > 0))) {
                return true;
            }
        }

        var span = GetChildrenSpan();
        foreach (var child in span) {
            if (child._canInteract && child.IsInBounds(globalPosition)) {
                return true;
            }
        }

        return false;
    }

    internal void GetBoundsInParent(out float minX, out float minY, out float maxX, out float maxY) {
        var (anchorX, anchorY) = Anchor.GetOffset(ContentWidth, ContentHeight);
        var transform = Transform2D.Create(X, Y, ScaleX, ScaleY, Rotation, anchorX, anchorY);
        GetTransformedBounds(transform, ContentWidth, ContentHeight, out minX, out minY, out maxX, out maxY);
    }

    /// <param name="pos">mouse position local to sprite</param>
    protected virtual bool CustomHitbox(Vector2i pos) {
        throw new MissingMethodException("Sprite must define override for CustomHitbox");
    }
}