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
            CollisionType.Custom => CustomHitbox(new Vector2i((int)(pos.X - _trueX), (int)(pos.Y - _trueY))),
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

    private Vector2 GlobalToLocal(Vector2i position) {
        if (_trueScale.X == 0f || _trueScale.Y == 0f) {
            return new Vector2(float.PositiveInfinity);
        }

        var scaled = new Vector2(
            (position.X - _trueX) / _trueScale.X + _anchorX,
            (position.Y - _trueY) / _trueScale.Y + _anchorY
        );
        var sin = MathF.Sin(-_trueRotation);
        var cos = MathF.Cos(-_trueRotation);
        var rotated = new Vector2(
            scaled.X * cos - scaled.Y * sin,
            scaled.X * sin + scaled.Y * cos
        );
        return rotated - new Vector2(_anchorX, _anchorY);
    }

    internal void GetBoundsInParent(out float minX, out float minY, out float maxX, out float maxY) {
        var (anchorX, anchorY) = Anchor.GetOffset(ContentWidth, ContentHeight);
        var anchor = new Vector2(anchorX, anchorY);
        var sin = MathF.Sin(Rotation);
        var cos = MathF.Cos(Rotation);

        minX = float.PositiveInfinity;
        minY = float.PositiveInfinity;
        maxX = float.NegativeInfinity;
        maxY = float.NegativeInfinity;

        IncludeTransformedCorner(Vector2.Zero, anchor, sin, cos, ref minX, ref minY, ref maxX, ref maxY);
        IncludeTransformedCorner(new Vector2(ContentWidth, 0f), anchor, sin, cos, ref minX, ref minY, ref maxX, ref maxY);
        IncludeTransformedCorner(new Vector2(ContentWidth, ContentHeight), anchor, sin, cos, ref minX, ref minY, ref maxX, ref maxY);
        IncludeTransformedCorner(new Vector2(0f, ContentHeight), anchor, sin, cos, ref minX, ref minY, ref maxX, ref maxY);
    }

    private void IncludeTransformedCorner(Vector2 corner, Vector2 anchor, float sin, float cos, ref float minX, ref float minY, ref float maxX, ref float maxY) {
        var anchored = corner + anchor;
        var rotated = new Vector2(
            anchored.X * cos - anchored.Y * sin,
            anchored.X * sin + anchored.Y * cos
        );
        var transformed = new Vector2(X + rotated.X * ScaleX, Y + rotated.Y * ScaleY);

        minX = Math.Min(minX, transformed.X);
        minY = Math.Min(minY, transformed.Y);
        maxX = Math.Max(maxX, transformed.X);
        maxY = Math.Max(maxY, transformed.Y);
    }
    
    /// <param name="pos">mouse position local to sprite</param>
    protected virtual bool CustomHitbox(Vector2i pos) {
        throw new MissingMethodException("Sprite must define override for CustomHitbox");
    }
}
